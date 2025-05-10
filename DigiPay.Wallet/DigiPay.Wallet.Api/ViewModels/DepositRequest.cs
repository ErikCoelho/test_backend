using System.ComponentModel.DataAnnotations;

namespace DigiPay.Wallet.Api.ViewModels
{
    public class DepositRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }
    }
} 