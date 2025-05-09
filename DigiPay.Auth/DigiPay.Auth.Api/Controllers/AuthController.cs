using DigiPay.Auth.Api.Models;
using DigiPay.Auth.Api.Services;
using DigiPay.Auth.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DigiPay.Auth.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;

        public AuthController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<ResultViewModel> Register([FromBody] RegisterRequest model)
        {
            return await _userService.RegisterUserAsync(model);
        }

        [HttpPost("login")]
        public async Task<ResultViewModel> Login([FromBody] LoginRequest model)
        {
            return await _userService.LoginAsync(model);
        }
    }
} 