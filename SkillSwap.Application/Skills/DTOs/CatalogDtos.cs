namespace SkillSwap.Application.Skills.DTOs;

public record CategoryDto(
    int Id,
    string Name,
    string? IconUrl
    );

public record SkillDto(
    int Id,
    string Name,
    int CategoryId,
    string CategoryName
    );
