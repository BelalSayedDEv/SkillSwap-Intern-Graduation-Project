namespace SkillSwap.Application.Identity.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsActiveAsync(Guid userId, CancellationToken cancellationToken = default);
}
