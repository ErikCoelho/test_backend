namespace DigiPay.Transaction.Api.Messaging.RabbitMQ
{
    public static class RabbitMQConstants
    {
        // Exchanges
        public const string TransactionExchange = "transaction.events";
        public const string WalletExchange = "wallet.events";

        // Queues
        public const string TransferRequestedQueue = "transaction.transfer.requested";
        public const string DepositRequestedQueue = "transaction.deposit.requested";
        public const string BalanceUpdatedQueue = "wallet.balance.updated";
        public const string TransactionCompletedQueue = "transaction.completed";

        // Routing Keys
        public const string TransferRequestedRoutingKey = "transaction.transfer.requested";
        public const string DepositRequestedRoutingKey = "transaction.deposit.requested";
        public const string BalanceUpdatedRoutingKey = "wallet.balance.updated";
        public const string TransactionCompletedRoutingKey = "transaction.completed";
    }
} 