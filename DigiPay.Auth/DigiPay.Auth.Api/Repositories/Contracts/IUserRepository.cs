using DigiPay.Auth.Api.Models;

namespace DigiPay.Auth.Api.Repositories.Contracts;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task RegisterAsync(User user);
    Task AddAsync(User user);
}

