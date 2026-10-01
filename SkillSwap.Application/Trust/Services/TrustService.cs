using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Identity.Interfaces;
using SkillSwap.Application.Trust.DTOs;
using SkillSwap.Application.Trust.Interfaces;
using SkillSwap.Domain.Trust;

namespace SkillSwap.Application.Trust.Services;

public class TrustService : ITrustService
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _users;

    public TrustService(IUnitOfWork uow, IUserRepository users)
    {
        _uow = uow;
        _users = users;
    }

    public async Task<Result<BlockedUserDto>> BlockAsync(Guid blockerId, BlockUserRequest request, CancellationToken cancellationToken = default)
    {
        if (request.BlockedId == blockerId)
            return Result<BlockedUserDto>.Failure(ErrorType.Validation, "You cannot block yourself.");

        if (!await _users.ExistsActiveAsync(request.BlockedId, cancellationToken))
            return Result<BlockedUserDto>.Failure(ErrorType.NotFound, "User not found.");

        if (await _uow.Blocks.IsBlockedAsync(blockerId, request.BlockedId, cancellationToken))
        {
            var mine = await _uow.Blocks.FindMineAsync(blockerId, request.BlockedId, cancellationToken);
            if (mine is not null)
                return Result<BlockedUserDto>.Failure(ErrorType.Conflict, "User is already blocked.");
            return Result<BlockedUserDto>.Failure(ErrorType.LogicError, "Action is not allowed between these users.");
        }

        var entity = new Block
        {
            BlockerId = blockerId,
            BlockedId = request.BlockedId
        };

        await _uow.Blocks.AddAsync(entity, cancellationToken);
        await _uow.CompleteAsync(cancellationToken);

        var profile = await _uow.Profiles.GetByUserIdAsync(request.BlockedId, cancellationToken);
        return Result<BlockedUserDto>.Success(new BlockedUserDto(entity.Id, entity.BlockedId, profile?.FullName ?? string.Empty, entity.CreatedAt));
    }

    public async Task<Result> UnblockAsync(Guid blockerId, Guid blockedId, CancellationToken cancellationToken = default)
    {
        var entity = await _uow.Blocks.FindMineAsync(blockerId, blockedId, cancellationToken);
        if (entity is null)
            return Result.Failure(ErrorType.NotFound, "Block not found.");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;

        await _uow.CompleteAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<BlockedUserDto>>> ListMyBlocksAsync(Guid blockerId, CancellationToken cancellationToken = default)
    {
        var items = await _uow.Blocks.ListMineAsync(blockerId, cancellationToken);
        return Result<IReadOnlyList<BlockedUserDto>>.Success(
            items.Select(b => new BlockedUserDto(b.Id, b.BlockedId, b.BlockedUser?.Profile?.FullName ?? string.Empty, b.CreatedAt)).ToList());
    }

    public async Task<Result<ReportDto>> ReportAsync(Guid reporterId, ReportUserRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ReportedUserId == reporterId)
            return Result<ReportDto>.Failure(ErrorType.Validation, "You cannot report yourself.");

        var reason = request.Reason?.Trim() ?? string.Empty;
        if (reason.Length < 5 || reason.Length > 200)
            return Result<ReportDto>.Failure(ErrorType.Validation, "Reason must be between 5 and 200 characters.");
        if (request.Description is not null && request.Description.Length > 1000)
            return Result<ReportDto>.Failure(ErrorType.Validation, "Description must be at most 1000 characters.");

        if (!await _users.ExistsActiveAsync(request.ReportedUserId, cancellationToken))
            return Result<ReportDto>.Failure(ErrorType.NotFound, "User not found.");

        if (await _uow.Reports.ExistsPendingAsync(reporterId, request.ReportedUserId, cancellationToken))
            return Result<ReportDto>.Failure(ErrorType.Conflict, "You already have a pending report against this user.");

        var entity = new Report
        {
            ReporterId = reporterId,
            ReportedUserId = request.ReportedUserId,
            Reason = reason,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Status = ReportStatus.Pending
        };

        await _uow.Reports.AddAsync(entity, cancellationToken);
        await _uow.CompleteAsync(cancellationToken);

        return Result<ReportDto>.Success(Map(entity, string.Empty, string.Empty));
    }

    public async Task<Result<PagedResult<ReportDto>>> ListReportsAsync(ReportStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return Result<PagedResult<ReportDto>>.Failure(ErrorType.Validation, "Page must be >= 1.");
        if (pageSize is < 1 or > 50)
            return Result<PagedResult<ReportDto>>.Failure(ErrorType.Validation, "PageSize must be between 1 and 50.");

        var paged = await _uow.Reports.GetPagedAsync(status, page, pageSize, cancellationToken);
        return Result<PagedResult<ReportDto>>.Success(new PagedResult<ReportDto>
        {
            Items = paged.Items.Select(r => Map(r,
                r.Reporter?.Profile?.FullName ?? string.Empty,
                r.ReportedUser?.Profile?.FullName ?? string.Empty)).ToList(),
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        });
    }

    public async Task<Result<ReportDto>> ResolveAsync(Guid id, ResolveReportRequest request, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(request.Status) || request.Status == ReportStatus.Pending)
            return Result<ReportDto>.Failure(ErrorType.Validation, "Status must be Resolved or Dismissed.");

        var entity = await _uow.Reports.GetPendingByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result<ReportDto>.Failure(ErrorType.NotFound, "Pending report not found.");

        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;

        await _uow.CompleteAsync(cancellationToken);

        return Result<ReportDto>.Success(Map(entity,
            entity.Reporter?.Profile?.FullName ?? string.Empty,
            entity.ReportedUser?.Profile?.FullName ?? string.Empty));
    }

    private static ReportDto Map(Report r, string reporterName, string reportedName)
    {
        return new ReportDto(r.Id, r.ReporterId, reporterName, r.ReportedUserId, reportedName,
            r.Reason, r.Description, r.Status, r.Status.ToString(), r.CreatedAt);
    }
}
