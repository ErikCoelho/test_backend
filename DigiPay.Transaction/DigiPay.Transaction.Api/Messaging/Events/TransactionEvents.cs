using System;

namespace DigiPay.Transaction.Api.Messaging.Events
{
    // Evento enviado quando uma transferência é solicitada
    public class TransferRequestedEvent
    {
        public Guid TransactionId { get; set; }
        public Guid SourceWalletId { get; set; }
        public Guid DestinationWalletId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }

        public TransferRequestedEvent(Guid transactionId, Guid sourceWalletId, Guid destinationWalletId, decimal amount, string description)
        {
            TransactionId = transactionId;
            SourceWalletId = sourceWalletId;
            DestinationWalletId = destinationWalletId;
            Amount = amount;
            Description = description;
            Timestamp = DateTime.UtcNow;
        }
    }

    // Evento enviado quando um depósito é solicitado
    public class DepositRequestedEvent
    {
        public Guid TransactionId { get; set; }
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }

        public DepositRequestedEvent(Guid transactionId, Guid walletId, decimal amount)
        {
            TransactionId = transactionId;
            WalletId = walletId;
            Amount = amount;
            Timestamp = DateTime.UtcNow;
        }
    }

    // Evento recebido quando um saldo é atualizado
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

        // Para desserialização
        public BalanceUpdatedEvent() { }
    }

    // Evento enviado quando uma transação é finalizada
    public class TransactionCompletedEvent
    {
        public Guid TransactionId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

        public TransactionCompletedEvent(Guid transactionId, bool success, string message)
        {
            TransactionId = transactionId;
            Success = success;
            Message = message;
            Timestamp = DateTime.UtcNow;
        }
    }
} 