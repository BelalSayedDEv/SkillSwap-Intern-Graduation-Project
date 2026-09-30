using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Skills.DTOs;
using SkillSwap.Application.Skills.Interfaces;
using SkillSwap.Domain.Skills;

namespace SkillSwap.Application.Skills.Services;

public class MySkillsService : IMySkillsService
{
    private readonly IUnitOfWork _uow;

    public MySkillsService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<MySkillDto>> AddAsync(Guid userId, AddMySkillRequest request, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(request.Type))
            return Result<MySkillDto>.Failure(ErrorType.Validation, "Invalid skill type.");

        if (!Enum.IsDefined(request.Level))
            return Result<MySkillDto>.Failure(ErrorType.Validation, "Invalid skill level.");

        var skill = await _uow.Skills.GetByIdAsync(request.SkillId, cancellationToken);

        if (skill is null || skill.IsApproved == false || skill.IsDeleted)
            return Result<MySkillDto>.Failure(ErrorType.Validation, "Skill is not available.");

        if (await _uow.UserSkills.ExistsAsync(userId, request.SkillId, request.Type, cancellationToken))
            return Result<MySkillDto>.Failure(ErrorType.Conflict, "Skill already added with the same type.");

        var entity = new UserSkill
        {
            UserId = userId,
            SkillId = request.SkillId,
            Type = request.Type,
            Level = request.Level
        };

        await _uow.UserSkills.AddAsync(entity, cancellationToken);
        await _uow.CompleteAsync(cancellationToken);

        var category = await _uow.SkillCategories.GetByIdAsync(skill.CategoryId, cancellationToken);
        return Result<MySkillDto>.Success(Map(entity, skill.Name, category?.Name ?? string.Empty));
    }

    public async Task<Result> RemoveAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _uow.UserSkills.GetUserSkillAsync(userId, id, cancellationToken);
        if (entity is null)
            return Result.Failure(ErrorType.NotFound, "Skill not found.");

        _uow.UserSkills.Remove(entity);
        await _uow.CompleteAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<MySkillDto>>> ListMineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var items = await _uow.UserSkills.ListByUserAsync(userId, cancellationToken);
        var dtos = items.Select(x => Map(x, x.Skill.Name, x.Skill.Category?.Name ?? string.Empty)).ToList();
        return Result<IReadOnlyList<MySkillDto>>.Success(dtos);
    }

    private static MySkillDto Map(UserSkill x, string skillName, string categoryName)
    {
        return new MySkillDto(x.Id, x.SkillId, skillName, categoryName, x.Type, x.Type.ToString(), x.Level, x.Level.ToString());
    }
}
