using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Skills.DTOs;

namespace SkillSwap.Application.Skills.Interfaces;

public interface ISkillCatalogService
{
    Task<Result<IReadOnlyList<CategoryDto>>> ListCategoriesAsync(CancellationToken cancellationToken = default);
    Task<Result<PagedResult<SkillDto>>> ListApprovedSkillsAsync(int? categoryId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<SkillDto>>> SearchSkillsAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default);
}
