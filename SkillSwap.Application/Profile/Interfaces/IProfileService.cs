using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Profile.DTOs;

namespace SkillSwap.Application.Profile.Interfaces;

public interface IProfileService
{
    Task<Result<MyProfileDto>> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<MyProfileDto>> UpdateMyProfileAsync(Guid userId, UpdateMyProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result> CompleteOnboardingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<AvailabilityDto>>> ListMyAvailabilitiesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<AvailabilityDto>> SetAvailabilityAsync(Guid userId, SetAvailabilityRequest request, CancellationToken cancellationToken = default);
    Task<Result> RemoveAvailabilityAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
}
