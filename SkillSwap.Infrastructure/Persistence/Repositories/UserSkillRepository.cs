using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Skills.Interfaces;
using SkillSwap.Domain.Skills;
using SkillSwap.Infrastructure.Persistence;
using SkillSwap.Infrastructure.Persistence.Repositories;

namespace SkillSwap.Infrastructure.Persistence.Repositories;

public class UserSkillRepository : Repository<UserSkill>, IUserSkillRepository
{
    public UserSkillRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<UserSkill>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _set.AsNoTracking()
            .Include(x => x.Skill).ThenInclude(s => s.Category)
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Skill.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserSkill?> GetUserSkillAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _set
            .Include(x => x.Skill).ThenInclude(s => s.Category)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userId, int skillId, SkillType type, CancellationToken cancellationToken = default)
    {
        return await _set.AnyAsync(x => x.UserId == userId && x.SkillId == skillId && x.Type == type, cancellationToken);
    }
}
