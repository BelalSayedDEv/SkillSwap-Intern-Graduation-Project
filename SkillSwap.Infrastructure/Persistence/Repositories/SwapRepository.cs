using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Matches.Interfaces;
using SkillSwap.Domain.Matches;
using SkillSwap.Infrastructure.Persistence;
using SkillSwap.Infrastructure.Persistence.Repositories;

namespace SkillSwap.Infrastructure.Persistence.Repositories;

public class SwapRepository : Repository<SwapRequest>, ISwapRepository
{
    public SwapRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<SwapRequest>> ListMineAsync(Guid userId, string box, SwapStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _set.AsNoTracking()
            .Include(s => s.Sender).ThenInclude(u => u.Profile)
            .Include(s => s.Receiver).ThenInclude(u => u.Profile)
            .Include(s => s.SkillOffered)
            .Include(s => s.SkillWanted)
            .Where(s => s.SenderId == userId || s.ReceiverId == userId);

        if (box == "sent")
            query = query.Where(s => s.SenderId == userId);
        else if (box == "received")
            query = query.Where(s => s.ReceiverId == userId);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<SwapRequest> { Items = items, Page = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<SwapRequest?> GetMineAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _set
            .Include(s => s.Sender).ThenInclude(u => u.Profile)
            .Include(s => s.Receiver).ThenInclude(u => u.Profile)
            .Include(s => s.SkillOffered)
            .Include(s => s.SkillWanted)
            .FirstOrDefaultAsync(s => s.Id == id && (s.SenderId == userId || s.ReceiverId == userId), cancellationToken);
    }
}
