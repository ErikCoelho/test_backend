using DigiPay.Auth.Api.Models;

namespace DigiPay.Auth.Api.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        bool ValidateToken(string token);
    }
} 