using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Skills.DTOs;
using SkillSwap.Application.Skills.Interfaces;
using SkillSwap.Domain.Skills;

namespace SkillSwap.Application.Skills.Services;

public class SkillModerationService : ISkillModerationService
{
    private readonly IUnitOfWork _uow;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SkillModerationService> _logger;

    public SkillModerationService(IUnitOfWork uow, IMemoryCache cache, ILogger<SkillModerationService> logger)
    {
        _uow = uow;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<RequestedSkillDto>> RequestSkillAsync(RequestSkillRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (name.Length < 2 || name.Length > 100)
            return Result<RequestedSkillDto>.Failure(ErrorType.Validation, "Skill name must be between 2 and 100 characters.");

        var category = await _uow.SkillCategories.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null || category.IsDeleted)
            return Result<RequestedSkillDto>.Failure(ErrorType.Validation, "Category does not exist.");

        if (await _uow.Skills.ExistsWithNameAsync(request.CategoryId, name, cancellationToken))
            return Result<RequestedSkillDto>.Failure(ErrorType.Conflict, "This skill already exists. Add it from the catalog instead.");

        var entity = new Skill
        {
            Name = name,
            CategoryId = request.CategoryId,
            IsApproved = false
        };

        await _uow.Skills.AddAsync(entity, cancellationToken);

        await _uow.CompleteAsync(cancellationToken);


        return Result<RequestedSkillDto>.Success(new RequestedSkillDto(entity.Id, entity.Name, entity.CategoryId, category.Name, entity.CreatedAt));
    }

    public async Task<Result<PagedResult<PendingSkillDto>>> ListPendingAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return Result<PagedResult<PendingSkillDto>>.Failure(ErrorType.Validation, "Page must be >= 1.");
        if (pageSize is < 1 or > 50)
            return Result<PagedResult<PendingSkillDto>>.Failure(ErrorType.Validation, "PageSize must be between 1 and 50.");

        var paged = await _uow.Skills.GetPendingPagedAsync(page, pageSize, cancellationToken);
        return Result<PagedResult<PendingSkillDto>>.Success(new PagedResult<PendingSkillDto>
        {
            Items = paged.Items.Select(s => new PendingSkillDto(s.Id, s.Name, s.CategoryId, s.Category.Name, s.CreatedAt)).ToList(),
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        });
    }

    public async Task<Result<SkillDto>> ApproveAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _uow.Skills.GetPendingByIdAsync(id, cancellationToken);

        if (entity is null)
            return Result<SkillDto>.Failure(ErrorType.NotFound, "Pending skill request not found.");

        entity.IsApproved = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _uow.CompleteAsync(cancellationToken);

        SkillCatalogCacheKeys.InvalidateCatalog(_cache);

        _logger.LogInformation("cache.invalidate approve {SkillId}", id);

        return Result<SkillDto>.Success(new SkillDto(entity.Id, entity.Name, entity.CategoryId, entity.Category.Name));
    }

    public async Task<Result> RejectAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _uow.Skills.GetPendingByIdAsync(id, cancellationToken);
        if (entity is null)
            return Result.Failure(ErrorType.NotFound, "Pending skill request not found.");

        var now = DateTime.UtcNow;
        entity.IsDeleted = true;
        entity.DeletedAt = now;
        entity.UpdatedAt = now;

        await _uow.CompleteAsync(cancellationToken);

        SkillCatalogCacheKeys.InvalidateCatalog(_cache);
        _logger.LogInformation("cache.invalidate reject {SkillId}", id);

        return Result.Success();
    }
}
