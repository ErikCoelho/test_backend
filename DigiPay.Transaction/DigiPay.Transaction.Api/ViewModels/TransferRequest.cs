using System;
using System.ComponentModel.DataAnnotations;

namespace DigiPay.Transaction.Api.ViewModels
{
    public class TransferRequest
    {
        [Required]
        public Guid SourceWalletId { get; set; }

        [Required]
        public Guid DestinationWalletId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;
    }
} 