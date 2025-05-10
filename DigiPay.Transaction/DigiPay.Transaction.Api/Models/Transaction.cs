using System;

namespace DigiPay.Transaction.Api.Models
{
    public enum TransactionType
    {
        Deposit,
        Transfer
    }

    public class Transaction
    {
        public Guid Id { get; private set; }
        public Guid WalletId { get; private set; }
        public Guid? ReceiverWalletId { get; private set; }
        public decimal Amount { get; private set; }
        public TransactionType Type { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string Description { get; private set; }

        public Transaction(Guid walletId, decimal amount, TransactionType type, string description, Guid? receiverWalletId = null)
        {
            Id = Guid.NewGuid();
            WalletId = walletId;
            Amount = amount;
            Type = type;
            Description = description;
            ReceiverWalletId = receiverWalletId;
            CreatedAt = DateTime.UtcNow;
        }

        // For EF Core
        protected Transaction() { }
    }
} 