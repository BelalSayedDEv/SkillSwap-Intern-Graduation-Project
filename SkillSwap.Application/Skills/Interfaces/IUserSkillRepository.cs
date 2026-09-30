using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Domain.Skills;

namespace SkillSwap.Application.Skills.Interfaces;

public interface IUserSkillRepository : IRepository<UserSkill>
{
    Task<IReadOnlyList<UserSkill>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserSkill?> GetUserSkillAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, int skillId, SkillType type, CancellationToken cancellationToken = default);
}
