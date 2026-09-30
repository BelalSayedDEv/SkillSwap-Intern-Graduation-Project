namespace SkillSwap.Application.Skills.DTOs;

public record RequestSkillRequest(
    string Name,
    int CategoryId
    );

public record RequestedSkillDto(
    int Id,
    string Name,
    int CategoryId,
    string CategoryName,
    DateTime RequestedAt
    );

public record PendingSkillDto(
    int Id,
    string Name,
    int CategoryId,
    string CategoryName,
    DateTime RequestedAt
    );
