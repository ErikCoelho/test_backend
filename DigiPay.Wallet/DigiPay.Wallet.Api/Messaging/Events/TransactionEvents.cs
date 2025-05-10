using System;

namespace DigiPay.Wallet.Api.Messaging.Events
{
    public class TransferRequestedEvent
    {
        public Guid TransactionId { get; set; }
        public Guid SourceWalletId { get; set; }
        public Guid DestinationWalletId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }

        public TransferRequestedEvent() { }
    }

    public class DepositRequestedEvent
    {
        public Guid TransactionId { get; set; }
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }

        public DepositRequestedEvent() { }
    }

    public class BalanceUpdatedEvent
    {
        public Guid TransactionId { get; set; }
        public Guid WalletId { get; set; }
        public decimal NewBalance { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

        public BalanceUpdatedEvent(Guid transactionId, Guid walletId, decimal newBalance, bool success, string message)
        {
            TransactionId = transactionId;
            WalletId = walletId;
            NewBalance = newBalance;
            Success = success;
            Message = message;
            Timestamp = DateTime.UtcNow;
        }
    }

    public class TransactionCompletedEvent
    {
        public Guid TransactionId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

        public TransactionCompletedEvent() { }
    }
} 