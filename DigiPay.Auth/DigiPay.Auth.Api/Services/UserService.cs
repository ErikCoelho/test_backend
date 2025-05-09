using DigiPay.Auth.Api.Models;
using DigiPay.Auth.Api.Repositories.Contracts;
using DigiPay.Auth.Api.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace DigiPay.Auth.Api.Services
{
    public class UserService
    {
        private readonly IJwtService _jwtService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IJwtService jwtService, IUserRepository userRepository, ILogger<UserService> logger)
        {
            _jwtService = jwtService;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<ResultViewModel> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    return new ResultViewModel(false, "Usuário não encontrado", string.Empty);
                }

                return new ResultViewModel(true, "Usuário encontrado", user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "N7362 - Falha interna no servidor");
                return new ResultViewModel(false, "N7362 - Falha interna no servidor", null);
            }
        }

        public async Task<ResultViewModel> RegisterUserAsync(RegisterRequest model)
        {
            try
            {
                if (await _userRepository.EmailExistsAsync(model.Email))
                {
                    return new ResultViewModel(false, "Email já cadastrado", string.Empty);
                }

                string passwordHash = HashPassword(model.Password);

                var newUser = new User(model.Username, model.Email, passwordHash);

                await _userRepository.AddAsync(newUser);
                return new ResultViewModel(true, "Usuário cadastrado com sucesso", string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "280LQ - Falha ao registrar usuário: {@Model}", model);
                return new ResultViewModel(false, "280LQ - Falha interna no servidor", string.Empty);
            }
        }

        public async Task<ResultViewModel> LoginAsync(LoginRequest model)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(model.Email);
                
                if (user == null || !await ValidateCredentialsAsync(user.Email, model.Password))
                {
                    return new ResultViewModel(false, "Email ou senha inválidos", string.Empty);
                }

                var token = _jwtService.GenerateToken(user);
                var expirationTime = DateTime.Now.AddMinutes(60);

                return new ResultViewModel(true, "Login realizado com sucesso", new { Token = token, Expiration = expirationTime });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "6529H - Falha ao fazer login: {@Model}", model);
                return new ResultViewModel(false, "6529H - Falha interna no servidor", string.Empty);
            }
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            string passwordHash = HashPassword(password);
            return user.PasswordHash == passwordHash;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
} 