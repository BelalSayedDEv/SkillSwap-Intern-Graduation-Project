using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Skills.DTOs;
using SkillSwap.Application.Skills.Interfaces;
using System.Collections.Concurrent;

namespace SkillSwap.Application.Skills.Services;

public class SkillCatalogService : ISkillCatalogService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _keyLocks = new();

    private readonly IUnitOfWork _uow;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SkillCatalogService> _logger;

    public SkillCatalogService(IUnitOfWork uow, IMemoryCache cache, ILogger<SkillCatalogService> logger)
    {
        _uow = uow;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> ListCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var key = SkillCatalogCacheKeys.CategoriesKey;

        if (_cache.TryGetValue(key, out IReadOnlyList<CategoryDto>? cached) && cached is not null)
        {
            _logger.LogInformation("cache.hit {Key}", key);
            return Result<IReadOnlyList<CategoryDto>>.Success(cached);
        }

        var gate = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        await gate.WaitAsync(cancellationToken);

        try
        {
            if (_cache.TryGetValue(key, out cached) && cached is not null)
            {
                _logger.LogInformation("cache.hit-after-wait {Key}", key);
                return Result<IReadOnlyList<CategoryDto>>.Success(cached);
            }

            _logger.LogInformation("cache.miss {Key}", key);

            var categories = await _uow.SkillCategories.ListOrderedAsync(cancellationToken);
            var dtos = categories.Select(c => new CategoryDto(c.Id, c.Name, c.IconUrl)).ToList();


            _cache.Set(key, (IReadOnlyList<CategoryDto>)dtos, SkillCatalogCacheKeys.CategoriesTtl);
            return Result<IReadOnlyList<CategoryDto>>.Success(dtos);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<Result<PagedResult<SkillDto>>> ListApprovedSkillsAsync(int? categoryId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var paging = NormalizePaging(page, pageSize);
        if (paging is not null) return paging;

        var version = SkillCatalogCacheKeys.ReadApprovedVersion(_cache);

        var key = SkillCatalogCacheKeys.BuildApprovedKey(version, categoryId, page, pageSize);

        if (_cache.TryGetValue(key, out PagedResult<SkillDto>? cached) && cached is not null)
        {
            _logger.LogInformation("cache.hit {Key}", key);
            return Result<PagedResult<SkillDto>>.Success(cached);
        }

        var gate = _keyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        await gate.WaitAsync(cancellationToken);

        try
        {
            if (_cache.TryGetValue(key, out cached) && cached is not null)
            {
                _logger.LogInformation("cache.hit-after-wait {Key}", key);
                return Result<PagedResult<SkillDto>>.Success(cached);
            }

            _logger.LogInformation("cache.miss {Key}", key);

            var paged = await _uow.Skills.GetPagedApprovedAsync(categoryId, page, pageSize, cancellationToken);

            var result = MapSkills(paged);

            _cache.Set(key, result, SkillCatalogCacheKeys.ApprovedPageTtl);

            return Result<PagedResult<SkillDto>>.Success(result);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<Result<PagedResult<SkillDto>>> SearchSkillsAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
            return Result<PagedResult<SkillDto>>.Failure(ErrorType.Validation, "Search query must be at least 2 characters.");

        var paging = NormalizePaging(page, pageSize);
        if (paging is not null) return paging;

        var paged = await _uow.Skills.SearchApprovedAsync(query.Trim(), page, pageSize, cancellationToken);
        return Result<PagedResult<SkillDto>>.Success(MapSkills(paged));
    }

    // helpers
    private static Result<PagedResult<SkillDto>>? NormalizePaging(int page, int pageSize)
    {
        if (page < 1)
            return Result<PagedResult<SkillDto>>.Failure(ErrorType.Validation, "Page must be >= 1.");
        if (pageSize is < 1 or > 50)
            return Result<PagedResult<SkillDto>>.Failure(ErrorType.Validation, "PageSize must be between 1 and 50.");
        return null;
    }

    private static PagedResult<SkillDto> MapSkills(PagedResult<Domain.Skills.Skill> paged)
    {
        return new PagedResult<SkillDto>
        {
            Items = paged.Items.Select(s => new SkillDto(s.Id, s.Name, s.CategoryId, s.Category.Name)).ToList(),
            Page = paged.Page,
            PageSize = paged.PageSize,
            TotalCount = paged.TotalCount
        };
    }
}
