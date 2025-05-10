using DigiPay.Wallet.Api.Services;
using DigiPay.Wallet.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigiPay.Wallet.Api.Controllers
{
    [ApiController]
    [Route("api/wallet")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly WalletService _walletService;
        private readonly JwtService _jwtService;
        private readonly ILogger<WalletController> _logger;

        public WalletController(
            WalletService walletService,
            JwtService jwtService,
            ILogger<WalletController> logger)
        {
            _walletService = walletService;
            _jwtService = jwtService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetBalance()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var result = await _walletService.GetBalanceAsync(userId);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get balance");
                return StatusCode(500, new ResultViewModel(false, "Internal server error", null));
            }
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var result = await _walletService.AddFundsAsync(userId, request.Amount);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deposit funds");
                return StatusCode(500, new ResultViewModel(false, "Internal server error", null));
            }
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var result = await _walletService.TransferFundsAsync(
                    userId,
                    request.DestinationUserId,
                    request.Amount,
                    request.Description);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to transfer funds");
                return StatusCode(500, new ResultViewModel(false, "Internal server error", null));
            }
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var result = await _walletService.GetTransactionsAsync(userId, startDate, endDate);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get transactions");
                return StatusCode(500, new ResultViewModel(false, "Internal server error", null));
            }
        }

        private Guid GetUserIdFromToken()
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                throw new UnauthorizedAccessException("Authorization header is missing or invalid");
            }

            return _jwtService.GetUserIdFromToken(authHeader);
        }
    }
} 