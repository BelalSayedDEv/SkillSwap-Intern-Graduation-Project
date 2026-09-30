using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Domain.Skills;

namespace SkillSwap.Application.Skills.Interfaces;

public interface ISkillCategoryRepository : IRepository<SkillCategory>
{
    Task<IReadOnlyList<SkillCategory>> ListOrderedAsync(CancellationToken cancellationToken = default);
}

public interface ISkillRepository : IRepository<Skill>
{
    Task<PagedResult<Skill>> GetPagedApprovedAsync(int? categoryId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<Skill>> SearchApprovedAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNameAsync(int categoryId, string name, CancellationToken cancellationToken = default);
    Task<PagedResult<Skill>> GetPendingPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Skill?> GetPendingByIdAsync(int id, CancellationToken cancellationToken = default);
}
