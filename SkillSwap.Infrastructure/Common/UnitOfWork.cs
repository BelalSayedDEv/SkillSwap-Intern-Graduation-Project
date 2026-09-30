using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Skills.Interfaces;
using SkillSwap.Infrastructure.Persistence;

namespace SkillSwap.Infrastructure.Common;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(
        AppDbContext context,
        ISkillCategoryRepository skillCategories,
        ISkillRepository skills,
        IUserSkillRepository userSkills)
    {
        _context = context;
        SkillCategories = skillCategories;
        Skills = skills;
        UserSkills = userSkills;
    }

    public ISkillCategoryRepository SkillCategories { get; }
    public ISkillRepository Skills { get; }
    public IUserSkillRepository UserSkills { get; }

    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
