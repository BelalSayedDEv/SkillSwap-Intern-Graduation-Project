using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Profile.DTOs;
using SkillSwap.Application.Profile.Interfaces;

namespace SkillSwap.Application.Profile.Services;

public class ProfileService : IProfileService
{
    private readonly IUnitOfWork _uow;

    public ProfileService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<MyProfileDto>> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await _uow.Profiles.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
            return Result<MyProfileDto>.Failure(ErrorType.NotFound, "Profile not found.");

        return Result<MyProfileDto>.Success(Map(profile));
    }

    public async Task<Result<MyProfileDto>> UpdateMyProfileAsync(Guid userId, UpdateMyProfileRequest request, CancellationToken cancellationToken = default)
    {
        if (request.FullName is not null && (request.FullName.Trim().Length < 2 || request.FullName.Trim().Length > 100))
            return Result<MyProfileDto>.Failure(ErrorType.Validation, "FullName must be between 2 and 100 characters.");
        if (request.Bio is not null && request.Bio.Length > 1000)
            return Result<MyProfileDto>.Failure(ErrorType.Validation, "Bio must be at most 1000 characters.");
        if (request.ProfilePictureUrl is not null && request.ProfilePictureUrl.Length > 500)
            return Result<MyProfileDto>.Failure(ErrorType.Validation, "ProfilePictureUrl must be at most 500 characters.");
        if (request.Location is not null && request.Location.Length > 200)
            return Result<MyProfileDto>.Failure(ErrorType.Validation, "Location must be at most 200 characters.");
        if (request.Timezone is not null && request.Timezone.Length > 100)
            return Result<MyProfileDto>.Failure(ErrorType.Validation, "Timezone must be at most 100 characters.");

        var profile = await _uow.Profiles.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
            return Result<MyProfileDto>.Failure(ErrorType.NotFound, "Profile not found.");

        if (request.FullName is not null) profile.FullName = request.FullName.Trim();
        if (request.Bio is not null) profile.Bio = request.Bio;
        if (request.ProfilePictureUrl is not null) profile.ProfilePictureUrl = request.ProfilePictureUrl;
        if (request.Location is not null) profile.Location = request.Location;
        if (request.Timezone is not null) profile.Timezone = request.Timezone;
        if (request.Latitude.HasValue) profile.Latitude = request.Latitude.Value;
        if (request.Longitude.HasValue) profile.Longitude = request.Longitude.Value;
        profile.UpdatedAt = DateTime.UtcNow;

        await _uow.CompleteAsync(cancellationToken);
        return Result<MyProfileDto>.Success(Map(profile));
    }

    public async Task<Result> CompleteOnboardingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await _uow.Profiles.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
            return Result.Failure(ErrorType.NotFound, "Profile not found.");

        profile.IsOnboardingCompleted = true;
        profile.UpdatedAt = DateTime.UtcNow;

        await _uow.CompleteAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<AvailabilityDto>>> ListMyAvailabilitiesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var items = await _uow.Availabilities.ListByUserAsync(userId, cancellationToken);
        return Result<IReadOnlyList<AvailabilityDto>>.Success(
            items.Select(a => new AvailabilityDto(a.Id, a.DayOfWeek, a.StartTime, a.EndTime, a.IsActive)).ToList());
    }

    public async Task<Result<AvailabilityDto>> SetAvailabilityAsync(Guid userId, SetAvailabilityRequest request, CancellationToken cancellationToken = default)
    {
        if (request.DayOfWeek is < 0 or > 6)
            return Result<AvailabilityDto>.Failure(ErrorType.Validation, "DayOfWeek must be between 0 and 6.");
        if (request.EndTime <= request.StartTime)
            return Result<AvailabilityDto>.Failure(ErrorType.Validation, "EndTime must be after StartTime.");

        var existing = await _uow.Availabilities.ListByUserAsync(userId, cancellationToken);
        if (existing.Any(a => a.DayOfWeek == request.DayOfWeek && Overlaps(a.StartTime, a.EndTime, request.StartTime, request.EndTime)))
            return Result<AvailabilityDto>.Failure(ErrorType.Conflict, "Availability overlaps an existing slot.");

        var entity = new Domain.Profile.Availability
        {
            UserId = userId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsActive = true
        };

        await _uow.Availabilities.AddAsync(entity, cancellationToken);
        await _uow.CompleteAsync(cancellationToken);

        return Result<AvailabilityDto>.Success(new AvailabilityDto(entity.Id, entity.DayOfWeek, entity.StartTime, entity.EndTime, entity.IsActive));
    }

    public async Task<Result> RemoveAvailabilityAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var slot = await _uow.Availabilities.GetUserSlotAsync(userId, id, cancellationToken);
        if (slot is null)
            return Result.Failure(ErrorType.NotFound, "Availability slot not found.");

        slot.IsDeleted = true;
        slot.DeletedAt = DateTime.UtcNow;

        await _uow.CompleteAsync(cancellationToken);
        return Result.Success();
    }

    private static bool Overlaps(TimeOnly s1, TimeOnly e1, TimeOnly s2, TimeOnly e2)
    {
        return s1 < e2 && s2 < e1;
    }

    private static MyProfileDto Map(Domain.Profile.UserProfile p)
    {
        return new MyProfileDto(p.UserId, p.FullName, p.User?.Email ?? string.Empty, p.Bio,
            p.ProfilePictureUrl, p.Location, p.Timezone, p.IsOnboardingCompleted);
    }
}
