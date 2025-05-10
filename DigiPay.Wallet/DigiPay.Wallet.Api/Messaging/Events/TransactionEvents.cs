using System;

namespace DigiPay.Wallet.Api.Messaging.Events
{
    // Evento recebido quando uma transferência é solicitada
    public class TransferRequestedEvent
    {
        public Guid TransactionId { get; set; }
        public Guid SourceWalletId { get; set; }
        public Guid DestinationWalletId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }

        // Para desserialização
        public TransferRequestedEvent() { }
    }

    // Evento recebido quando um depósito é solicitado
    public class DepositRequestedEvent
    {
        public Guid TransactionId { get; set; }
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }

        // Para desserialização
        public DepositRequestedEvent() { }
    }

    // Evento enviado quando um saldo é atualizado
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

    // Evento recebido quando uma transação é finalizada
    public class TransactionCompletedEvent
    {
        public Guid TransactionId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }

        // Para desserialização
        public TransactionCompletedEvent() { }
    }
} 