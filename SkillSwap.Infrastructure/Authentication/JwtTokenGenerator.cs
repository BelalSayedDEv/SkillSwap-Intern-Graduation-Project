using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SkillSwap.Application.Identity.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SkillSwap.Infrastructure.Authentication
{

    public class JwtTokenGenerator : ITokenService
    {

        private readonly IOptionsMonitor<JwtSettings> _jwtOptions;

        public JwtTokenGenerator(IOptionsMonitor<JwtSettings> jwtOptions)
        {
            _jwtOptions = jwtOptions;
        }

        public string GenerateAccessToken(Guid userId, string email, string role)
        {
            // 1. Read CurrentValue inside the method for live-reload support
            var settings = _jwtOptions.CurrentValue;

            // 2. Setup the Symmetric Security Key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. Prepare the claims (using ClaimTypes.Role for proper [Authorize] mapping)
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
            };

            // 4. Assemble the Token Descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(settings.AccessTokenMinutes),
                Issuer = settings.Issuer,
                Audience = settings.Audience,
                SigningCredentials = creds
            };

            // 5. Create and write the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            // Cryptographically secure random bytes
            var randomNumber = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
