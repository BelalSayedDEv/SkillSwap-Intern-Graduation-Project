namespace SkillSwap.Application.Profile.DTOs;

public record MyProfileDto(
    Guid UserId,
    string FullName,
    string Email,
    string? Bio,
    string? ProfilePictureUrl,
    string? Location,
    string? Timezone,
    bool IsOnboardingCompleted
    );

public record UpdateMyProfileRequest(
    string? FullName,
    string? Bio,
    string? ProfilePictureUrl,
    string? Location,
    string? Timezone,
    decimal? Latitude,
    decimal? Longitude
    );

public record AvailabilityDto(
    Guid Id,
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive
    );

public record SetAvailabilityRequest(
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime
    );
