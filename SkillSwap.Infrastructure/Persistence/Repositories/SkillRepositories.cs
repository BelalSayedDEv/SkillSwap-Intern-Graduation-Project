using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Skills.Interfaces;
using SkillSwap.Domain.Skills;

namespace SkillSwap.Infrastructure.Persistence.Repositories;

public class SkillCategoryRepository : Repository<SkillCategory>, ISkillCategoryRepository
{
    public SkillCategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<SkillCategory>> ListOrderedAsync(CancellationToken cancellationToken = default)
    {
        return await _set.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);
    }
}



public class SkillRepository : Repository<Skill>, ISkillRepository
{
    public SkillRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Skill>> GetPagedApprovedAsync(int? categoryId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _set.AsNoTracking()
            .Include(s => s.Category)
            .Where(s => s.IsApproved);

        if (categoryId.HasValue)
            query = query.Where(s => s.CategoryId == categoryId.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query.OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Skill> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<PagedResult<Skill>> SearchApprovedAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var q = _set.AsNoTracking()
            .Include(s => s.Category)
            .Where(s => s.IsApproved && s.Name.Contains(query));

        var total = await q.CountAsync(cancellationToken);

        var items = await q.OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Skill> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<bool> ExistsWithNameAsync(int categoryId, string name, CancellationToken cancellationToken = default)
    {
        return await _set.AnyAsync(s => s.CategoryId == categoryId && s.Name == name, cancellationToken);
    }

    public async Task<PagedResult<Skill>> GetPendingPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _set.AsNoTracking()
            .Include(s => s.Category)
            .Where(s => !s.IsApproved);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Skill> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<Skill?> GetPendingByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _set
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsApproved && !s.IsDeleted, cancellationToken);
    }
}
