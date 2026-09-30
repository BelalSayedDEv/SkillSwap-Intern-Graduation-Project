namespace SkillSwap.Infrastructure.Extensions;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Identity.Interfaces;
using SkillSwap.Application.Skills.Interfaces;
using SkillSwap.Application.Skills.Services;
using SkillSwap.Domain.Identity;
using SkillSwap.Infrastructure.Authentication;
using SkillSwap.Infrastructure.Common;
using SkillSwap.Infrastructure.Persistence;
using SkillSwap.Infrastructure.Persistence.Repositories;
using SkillSwap.Infrastructure.Services.Auth;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(jwt => !string.IsNullOrWhiteSpace(jwt.SecretKey), "JWT Error: SecretKey is required. Use user-secrets or Jwt__SecretKey env.")
            .Validate(jwt => jwt.SecretKey.Length >= 32, "JWT Error: SecretKey must be >= 32 chars for HmacSha256.")
            .Validate(jwt => !string.IsNullOrWhiteSpace(jwt.Issuer), "JWT Error: Issuer is required.")
            .Validate(jwt => !string.IsNullOrWhiteSpace(jwt.Audience), "JWT Error: Audience is required.")
            .Validate(jwt => jwt.AccessTokenMinutes > 0, "JWT Error: AccessTokenMinutes must be greater than 0.")
            .Validate(jwt => jwt.RefreshTokenDays > 0, "JWT Error: RefreshTokenDays must be greater than 0.")
            .ValidateOnStart();

        services.AddSingleton<ITokenService, JwtTokenGenerator>();

        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<ISkillCategoryRepository, SkillCategoryRepository>();
        services.AddScoped<ISkillRepository, SkillRepository>();
        services.AddScoped<IUserSkillRepository, UserSkillRepository>();
        services.AddScoped<ISkillCatalogService, SkillCatalogService>();
        services.AddScoped<IMySkillsService, MySkillsService>();
        services.AddScoped<ISkillModerationService, SkillModerationService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}
