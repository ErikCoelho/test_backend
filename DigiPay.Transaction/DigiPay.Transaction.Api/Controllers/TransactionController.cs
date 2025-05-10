using DigiPay.Transaction.Api.Services;
using DigiPay.Transaction.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DigiPay.Transaction.Api.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(
            TransactionService transactionService,
            ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> ProcessTransfer([FromBody] TransferRequest request)
        {
            try
            {
                var result = await _transactionService.ProcessTransferAsync(
                    request.SourceWalletId,
                    request.DestinationWalletId,
                    request.Amount,
                    request.Description);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process transfer");
                return StatusCode(500, new ResultViewModel(false, "Internal server error", null));
            }
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> RegisterDeposit([FromBody] DepositRequest request)
        {
            try
            {
                var result = await _transactionService.RegisterDepositAsync(
                    request.WalletId,
                    request.Amount);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register deposit");
                return StatusCode(500, new ResultViewModel(false, "Internal server error", null));
            }
        }

        [HttpGet("wallet/{walletId}")]
        public async Task<IActionResult> GetTransactions(
            Guid walletId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var result = await _transactionService.GetTransactionsAsync(walletId, startDate, endDate);

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
    }
} 