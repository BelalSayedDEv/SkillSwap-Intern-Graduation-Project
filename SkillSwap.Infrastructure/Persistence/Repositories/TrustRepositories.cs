using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Trust.Interfaces;
using SkillSwap.Domain.Trust;
using SkillSwap.Infrastructure.Persistence;
using SkillSwap.Infrastructure.Persistence.Repositories;

namespace SkillSwap.Infrastructure.Persistence.Repositories;

public class BlockRepository : IBlockRepository
{
    private readonly AppDbContext _context;

    public BlockRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsBlockedAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Block>().AnyAsync(
            b => (b.BlockerId == userA && b.BlockedId == userB) || (b.BlockerId == userB && b.BlockedId == userA),
            cancellationToken);
    }

    public async Task<Block?> FindMineAsync(Guid blockerId, Guid blockedId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Block>().FirstOrDefaultAsync(
            b => b.BlockerId == blockerId && b.BlockedId == blockedId, cancellationToken);
    }

    public async Task<IReadOnlyList<Block>> ListMineAsync(Guid blockerId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Block>().AsNoTracking()
            .Include(b => b.BlockedUser).ThenInclude(u => u.Profile)
            .Where(b => b.BlockerId == blockerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Block block, CancellationToken cancellationToken = default)
    {
        await _context.Set<Block>().AddAsync(block, cancellationToken);
    }
}

public class ReportRepository : Repository<Report>, IReportRepository
{
    public ReportRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Report>> GetPagedAsync(ReportStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _set.AsNoTracking()
            .Include(r => r.Reporter).ThenInclude(u => u.Profile)
            .Include(r => r.ReportedUser).ThenInclude(u => u.Profile)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Report> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<Report?> GetPendingByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _set
            .Include(r => r.Reporter).ThenInclude(u => u.Profile)
            .Include(r => r.ReportedUser).ThenInclude(u => u.Profile)
            .FirstOrDefaultAsync(r => r.Id == id && r.Status == ReportStatus.Pending, cancellationToken);
    }

    public async Task<bool> ExistsPendingAsync(Guid reporterId, Guid reportedUserId, CancellationToken cancellationToken = default)
    {
        return await _set.AnyAsync(r => r.ReporterId == reporterId && r.ReportedUserId == reportedUserId && r.Status == ReportStatus.Pending, cancellationToken);
    }
}
