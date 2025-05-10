using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DigiPay.Wallet.Api.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Guid GetUserIdFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token is required");

            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = token.Substring(7);

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new ArgumentException("Invalid token: User ID claim not found");

            if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
                throw new ArgumentException("Invalid user ID format in token");

            return userId;
        }
    }
} 