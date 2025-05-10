using System;
using System.ComponentModel.DataAnnotations;

namespace DigiPay.Transaction.Api.ViewModels
{
    public class DepositRequest
    {
        [Required]
        public Guid WalletId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }
    }
} 