using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Identity.Interfaces;
using SkillSwap.Domain.Identity;
using SkillSwap.Infrastructure.Persistence;

namespace SkillSwap.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsActiveAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Id == userId && u.Status == UserStatus.Active, cancellationToken);
    }
}
