using Microsoft.AspNetCore.Authorization;
using SkillSwap.Domain.Identity;
using SkillSwap.Infrastructure.Authorization;

namespace SkillSwap.API.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddSkillSwapAuthorization(this IServiceCollection services)
        {

            services.AddScoped<IAuthorizationHandler, UserAuthorizationHandler>();

            services.AddAuthorization(options =>
            {

                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole(nameof(UserRole.Admin)));


                options.AddPolicy("ActiveUserOnly", policy =>
                    policy.AddRequirements(new UserRequirement()));
            });

            return services;
        }
    }
}
