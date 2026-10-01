using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Identity.Interfaces;
using SkillSwap.Application.Matches.DTOs;
using SkillSwap.Application.Matches.Interfaces;
using SkillSwap.Application.Subscriptions.Interfaces;
using SkillSwap.Domain.Matches;
using SkillSwap.Domain.Skills;

namespace SkillSwap.Application.Matches.Services;

public class SwapService : ISwapService
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _users;
    private readonly ISubscriptionService _subscriptions;

    public SwapService(IUnitOfWork uow, IUserRepository users, ISubscriptionService subscriptions)
    {
        _uow = uow;
        _users = users;
        _subscriptions = subscriptions;
    }

    public async Task<Result<SwapDto>> SendAsync(Guid senderId, SendSwapRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ReceiverId == senderId)
            return Result<SwapDto>.Failure(ErrorType.Validation, "You cannot send a swap request to yourself.");

        if (!await _users.ExistsActiveAsync(request.ReceiverId, cancellationToken))
            return Result<SwapDto>.Failure(ErrorType.NotFound, "Receiver not found.");

        if (await _uow.Blocks.IsBlockedAsync(senderId, request.ReceiverId, cancellationToken))
            return Result<SwapDto>.Failure(ErrorType.LogicError, "Swap is not allowed between these users.");

        if (!await _uow.UserSkills.ExistsAsync(senderId, request.SkillOfferedId, SkillType.Teach, cancellationToken))
            return Result<SwapDto>.Failure(ErrorType.Validation, "Offered skill must be one of your Teach skills.");

        var wanted = await _uow.Skills.GetByIdAsync(request.SkillWantedId, cancellationToken);
        if (wanted is null || wanted.IsApproved == false || wanted.IsDeleted)
            return Result<SwapDto>.Failure(ErrorType.Validation, "Wanted skill is not available.");

        var message = request.Message?.Trim();
        if (message is not null && message.Length > 1000)
            return Result<SwapDto>.Failure(ErrorType.Validation, "Message must be at most 1000 characters.");

        var entity = new SwapRequest
        {
            SenderId = senderId,
            ReceiverId = request.ReceiverId,
            SkillOfferedId = request.SkillOfferedId,
            SkillWantedId = request.SkillWantedId,
            Message = string.IsNullOrWhiteSpace(message) ? null : message,
            Status = SwapStatus.Pending
        };

        await _uow.Swaps.AddAsync(entity, cancellationToken);
        await _uow.CompleteAsync(cancellationToken);

        var created = await _uow.Swaps.GetMineAsync(senderId, entity.Id, cancellationToken);
        return Result<SwapDto>.Success(Map(created!));
    }

    public async Task<Result<SwapDto>> GetDetailsAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _uow.Swaps.GetMineAsync(userId, id, cancellationToken);
        if (entity is null)
            return Result<SwapDto>.Failure(ErrorType.NotFound, "Swap request not found.");

        return Result<SwapDto>.Success(Map(entity));
    }

    public async Task<Result<PagedResult<SwapDto>>> ListMineAsync(Guid userId, string box, SwapStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        box = (box ?? "all").ToLowerInvariant();
        if (box is not ("all" or "sent" or "received"))
            return Result<PagedResult<SwapDto>>.Failure(ErrorType.Validation, "Box must be all, sent or received.");
        if (page < 1)
            return Result<PagedResult<SwapDto>>.Failure(ErrorType.Validation, "Page must be >= 1.");
        if (pageSize is < 1 or > 50)
            return Result<PagedResult<SwapDto>>.Failure(ErrorType.Validation, "PageSize must be between 1 and 50.");

        var paged = await _uow.Swaps.ListMineAsync(userId, box, status, page, pageSize, cancellationToken);
        return Result<PagedResult<SwapDto>>.Success(new PagedResult<SwapDto>
        {
            Items = paged.Items.Select(Map).ToList(),
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        });
    }

    public async Task<Result<SwapDto>> AcceptAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _uow.Swaps.GetMineAsync(userId, id, cancellationToken);
        if (entity is null || entity.ReceiverId != userId)
            return Result<SwapDto>.Failure(ErrorType.NotFound, "Swap request not found.");
        if (entity.Status != SwapStatus.Pending)
            return Result<SwapDto>.Failure(ErrorType.LogicError, $"Only pending requests can be accepted (current: {entity.Status}).");

        var consume = await _subscriptions.ConsumeSessionAsync(userId, cancellationToken);
        if (!consume.IsSuccess)
            return Result<SwapDto>.Failure(consume.ErrorType, consume.ErrorMessage);

        entity.Status = SwapStatus.Accepted;
        entity.UpdatedAt = DateTime.UtcNow;

        await _uow.CompleteAsync(cancellationToken);

        return Result<SwapDto>.Success(Map(entity));
    }

    public async Task<Result<SwapDto>> RejectAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _uow.Swaps.GetMineAsync(userId, id, cancellationToken);
        if (entity is null || entity.ReceiverId != userId)
            return Result<SwapDto>.Failure(ErrorType.NotFound, "Swap request not found.");
        if (entity.Status != SwapStatus.Pending)
            return Result<SwapDto>.Failure(ErrorType.LogicError, $"Only pending requests can be rejected (current: {entity.Status}).");

        entity.Status = SwapStatus.Rejected;
        entity.UpdatedAt = DateTime.UtcNow;

        await _uow.CompleteAsync(cancellationToken);
        return Result<SwapDto>.Success(Map(entity));
    }

    public async Task<Result> CancelAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _uow.Swaps.GetMineAsync(userId, id, cancellationToken);
        if (entity is null || entity.SenderId != userId)
            return Result.Failure(ErrorType.NotFound, "Swap request not found.");
        if (entity.Status != SwapStatus.Pending)
            return Result.Failure(ErrorType.LogicError, $"Only pending requests can be cancelled (current: {entity.Status}).");

        entity.Status = SwapStatus.Cancelled;
        entity.UpdatedAt = DateTime.UtcNow;

        await _uow.CompleteAsync(cancellationToken);
        return Result.Success();
    }

    private static SwapDto Map(SwapRequest s)
    {
        return new SwapDto(s.Id, s.SenderId, s.Sender?.Profile?.FullName ?? string.Empty,
            s.ReceiverId, s.Receiver?.Profile?.FullName ?? string.Empty,
            s.SkillOfferedId, s.SkillOffered?.Name ?? string.Empty,
            s.SkillWantedId, s.SkillWanted?.Name ?? string.Empty,
            s.Status, s.Status.ToString(), s.Message, s.CreatedAt);
    }
}
