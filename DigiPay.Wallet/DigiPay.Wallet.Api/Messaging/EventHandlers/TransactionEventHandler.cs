using DigiPay.Wallet.Api.Messaging.Events;
using DigiPay.Wallet.Api.Messaging.RabbitMQ;
using DigiPay.Wallet.Api.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DigiPay.Wallet.Api.Messaging.EventHandlers
{
    public class TransactionEventHandler
    {
        private readonly WalletService _walletService;
        private readonly RabbitMQService _rabbitMQService;
        private readonly ILogger<TransactionEventHandler> _logger;

        public TransactionEventHandler(
            WalletService walletService,
            RabbitMQService rabbitMQService,
            ILogger<TransactionEventHandler> logger)
        {
            _walletService = walletService;
            _rabbitMQService = rabbitMQService;
            _logger = logger;

            // Inscrever-se nos eventos
            _rabbitMQService.SubscribeToTransferRequested(HandleTransferRequested);
            _rabbitMQService.SubscribeToDepositRequested(HandleDepositRequested);
        }

        private async Task HandleTransferRequested(TransferRequestedEvent @event)
        {
            _logger.LogInformation("Processando transferência: {TransactionId}, De: {SourceWalletId}, Para: {DestinationWalletId}, Valor: {Amount}",
                @event.TransactionId, @event.SourceWalletId, @event.DestinationWalletId, @event.Amount);

            try
            {
                // Processar a transferência usando o WalletService
                var result = await _walletService.TransferFundsAsync(
                    @event.SourceWalletId,
                    @event.DestinationWalletId,
                    @event.Amount,
                    @event.Description);

                // Obter o saldo atualizado para incluir no evento
                var walletInfo = await _walletService.GetBalanceAsync(@event.SourceWalletId);
                decimal newBalance = 0;

                if (walletInfo.Success && walletInfo.Data != null)
                {
                    // Extrair saldo do objeto dinâmico
                    newBalance = (decimal)((dynamic)walletInfo.Data).Balance;
                }

                // Publicar evento de atualização de saldo
                var balanceUpdatedEvent = new BalanceUpdatedEvent(
                    @event.TransactionId,
                    @event.SourceWalletId,
                    newBalance,
                    result.Success,
                    result.Message
                );

                _rabbitMQService.PublishBalanceUpdated(balanceUpdatedEvent);

                _logger.LogInformation("Transferência processada: {TransactionId}, Sucesso: {Success}",
                    @event.TransactionId, result.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar transferência: {TransactionId}", @event.TransactionId);

                // Publicar falha
                var balanceUpdatedEvent = new BalanceUpdatedEvent(
                    @event.TransactionId,
                    @event.SourceWalletId,
                    0,
                    false,
                    "Erro interno ao processar transferência"
                );

                _rabbitMQService.PublishBalanceUpdated(balanceUpdatedEvent);
            }
        }

        private async Task HandleDepositRequested(DepositRequestedEvent @event)
        {
            _logger.LogInformation("Processando depósito: {TransactionId}, Carteira: {WalletId}, Valor: {Amount}",
                @event.TransactionId, @event.WalletId, @event.Amount);

            try
            {
                // Processar o depósito usando o WalletService
                var result = await _walletService.AddFundsAsync(@event.WalletId, @event.Amount);

                // Obter o saldo atualizado para incluir no evento
                var walletInfo = await _walletService.GetBalanceAsync(@event.WalletId);
                decimal newBalance = 0;

                if (walletInfo.Success && walletInfo.Data != null)
                {
                    // Extrair saldo do objeto dinâmico
                    newBalance = (decimal)((dynamic)walletInfo.Data).Balance;
                }

                // Publicar evento de atualização de saldo
                var balanceUpdatedEvent = new BalanceUpdatedEvent(
                    @event.TransactionId,
                    @event.WalletId,
                    newBalance,
                    result.Success,
                    result.Message
                );

                _rabbitMQService.PublishBalanceUpdated(balanceUpdatedEvent);

                _logger.LogInformation("Depósito processado: {TransactionId}, Sucesso: {Success}",
                    @event.TransactionId, result.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar depósito: {TransactionId}", @event.TransactionId);

                // Publicar falha
                var balanceUpdatedEvent = new BalanceUpdatedEvent(
                    @event.TransactionId,
                    @event.WalletId,
                    0,
                    false,
                    "Erro interno ao processar depósito"
                );

                _rabbitMQService.PublishBalanceUpdated(balanceUpdatedEvent);
            }
        }
    }
} 