namespace SkillSwap.Application.Skills.DTOs;

using SkillSwap.Domain.Skills;

public record AddMySkillRequest(
    int SkillId,
    SkillType Type,
    SkillLevel Level
    );

public record MySkillDto(
    Guid Id,
    int SkillId,
    string SkillName,
    string CategoryName,
    SkillType Type,
    string TypeName,
    SkillLevel Level,
    string LevelName
    );
