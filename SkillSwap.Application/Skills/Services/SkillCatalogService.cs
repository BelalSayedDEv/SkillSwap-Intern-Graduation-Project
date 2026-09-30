using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Skills.DTOs;
using SkillSwap.Application.Skills.Interfaces;
using SkillSwap.Domain.Skills;

namespace SkillSwap.Application.Skills.Services;

public class SkillCatalogService : ISkillCatalogService
{
    private readonly IUnitOfWork _uow;

    public SkillCatalogService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> ListCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _uow.SkillCategories.ListOrderedAsync(cancellationToken);
        var dtos = categories.Select(c => new CategoryDto(c.Id, c.Name, c.IconUrl)).ToList();
        return Result<IReadOnlyList<CategoryDto>>.Success(dtos);
    }

    public async Task<Result<PagedResult<SkillDto>>> ListApprovedSkillsAsync(int? categoryId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var paging = NormalizePaging(page, pageSize);
        if (paging is not null) return paging;

        var paged = await _uow.Skills.GetPagedApprovedAsync(categoryId, page, pageSize, cancellationToken);
        return Result<PagedResult<SkillDto>>.Success(MapSkills(paged));
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

    private static PagedResult<SkillDto> MapSkills(PagedResult<Skill> paged)
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
