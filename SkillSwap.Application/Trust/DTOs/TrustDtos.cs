namespace SkillSwap.Application.Trust.DTOs;

using SkillSwap.Domain.Trust;

public record BlockUserRequest(
    Guid BlockedId
    );

public record BlockedUserDto(
    Guid Id,
    Guid BlockedId,
    string BlockedName,
    DateTime CreatedAt
    );

public record ReportUserRequest(
    Guid ReportedUserId,
    string Reason,
    string? Description
    );

public record ReportDto(
    Guid Id,
    Guid ReporterId,
    string ReporterName,
    Guid ReportedUserId,
    string ReportedName,
    string Reason,
    string? Description,
    ReportStatus Status,
    string StatusName,
    DateTime CreatedAt
    );

public record ResolveReportRequest(
    ReportStatus Status
    );
