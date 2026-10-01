using SkillSwap.Domain.Matches;

namespace SkillSwap.Application.Matches.DTOs;

public record SendSwapRequest(
    Guid ReceiverId,
    int SkillOfferedId,
    int SkillWantedId,
    string? Message
    );

public record SwapDto(
    Guid Id,
    Guid SenderId,
    string SenderName,
    Guid ReceiverId,
    string ReceiverName,
    int SkillOfferedId,
    string SkillOfferedName,
    int SkillWantedId,
    string SkillWantedName,
    SwapStatus Status,
    string StatusName,
    string? Message,
    DateTime CreatedAt
    );
