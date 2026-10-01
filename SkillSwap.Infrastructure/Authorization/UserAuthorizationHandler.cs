using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SkillSwap.Domain.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SkillSwap.Infrastructure.Authorization
{
    public class UserAuthorizationHandler : AuthorizationHandler<UserRequirement>
    {
        private readonly UserManager<ApplicationUser> userManager;

        public UserAuthorizationHandler(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserRequirement requirement)
        {
            // NOTE: JwtBearer maps "sub" -> NameIdentifier by default inbound mapping.
            var userId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            var user = await userManager.FindByIdAsync(userId);


            if (user != null && user.Status == UserStatus.Active && !user.IsDeleted)
            {
                context.Succeed(requirement);
            }
        }
    }
}
