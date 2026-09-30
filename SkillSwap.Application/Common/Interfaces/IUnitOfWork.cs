using SkillSwap.Application.Skills.Interfaces;

namespace SkillSwap.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ISkillCategoryRepository SkillCategories { get; }
    ISkillRepository Skills { get; }
    IUserSkillRepository UserSkills { get; }

    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}
