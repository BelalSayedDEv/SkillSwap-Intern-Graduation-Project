using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Skills.DTOs;

namespace SkillSwap.Application.Skills.Interfaces;

public interface IMySkillsService
{
    Task<Result<MySkillDto>> AddAsync(Guid userId, AddMySkillRequest request, CancellationToken cancellationToken = default);
    Task<Result> RemoveAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<MySkillDto>>> ListMineAsync(Guid userId, CancellationToken cancellationToken = default);
}
