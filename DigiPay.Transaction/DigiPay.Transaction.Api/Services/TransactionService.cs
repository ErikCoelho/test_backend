using DigiPay.Transaction.Api.Messaging.Events;
using DigiPay.Transaction.Api.Messaging.RabbitMQ;
using DigiPay.Transaction.Api.Models;
using DigiPay.Transaction.Api.Repositories.Contracts;
using DigiPay.Transaction.Api.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DigiPay.Transaction.Api.Services
{
    public class TransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly RabbitMQService _rabbitMQService;
        private readonly ILogger<TransactionService> _logger;
        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<ResultViewModel>> _pendingTransactions;

        public TransactionService(
            ITransactionRepository transactionRepository,
            RabbitMQService rabbitMQService,
            ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _rabbitMQService = rabbitMQService;
            _logger = logger;
            _pendingTransactions = new ConcurrentDictionary<Guid, TaskCompletionSource<ResultViewModel>>();

            // Assinar eventos de atualização de saldo
            _rabbitMQService.SubscribeToBalanceUpdated(HandleBalanceUpdated);
        }

        public async Task<ResultViewModel> ProcessTransferAsync(
            Guid sourceWalletId,
            Guid destinationWalletId,
            decimal amount,
            string description)
        {
            try
            {
                if (sourceWalletId == destinationWalletId)
                {
                    return new ResultViewModel(false, "Não é possível transferir para a mesma carteira", null);
                }

                if (amount <= 0)
                {
                    return new ResultViewModel(false, "O valor deve ser maior que zero", null);
                }

                // Criar a transação (no estado pendente)
                var transaction = new Models.Transaction(
                    sourceWalletId,
                    amount,
                    TransactionType.Transfer,
                    description,
                    destinationWalletId
                );

                await _transactionRepository.AddAsync(transaction);

                // Criar um TaskCompletionSource para esta transação
                var tcs = new TaskCompletionSource<ResultViewModel>();
                _pendingTransactions[transaction.Id] = tcs;

                // Publicar o evento para o serviço de carteira processar
                var transferEvent = new TransferRequestedEvent(
                    transaction.Id,
                    sourceWalletId,
                    destinationWalletId,
                    amount,
                    description
                );

                _rabbitMQService.PublishTransferRequested(transferEvent);

                // Aguardar a confirmação com timeout
                var completionTask = tcs.Task;
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
                
                var completedTask = await Task.WhenAny(completionTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    _pendingTransactions.TryRemove(transaction.Id, out _);
                    return new ResultViewModel(false, "Timeout na operação de transferência", null);
                }

                return await completionTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "T47KP - Falha ao processar transferência: {SourceWalletId}, {DestinationWalletId}, {Amount}",
                    sourceWalletId, destinationWalletId, amount);
                return new ResultViewModel(false, "T47KP - Falha interna no servidor", null);
            }
        }

        public async Task<ResultViewModel> RegisterDepositAsync(Guid walletId, decimal amount)
        {
            try
            {
                if (amount <= 0)
                {
                    return new ResultViewModel(false, "O valor deve ser maior que zero", null);
                }

                // Criar a transação (no estado pendente)
                var transaction = new Models.Transaction(
                    walletId,
                    amount,
                    TransactionType.Deposit,
                    "Depósito de fundos"
                );

                await _transactionRepository.AddAsync(transaction);

                // Criar um TaskCompletionSource para esta transação
                var tcs = new TaskCompletionSource<ResultViewModel>();
                _pendingTransactions[transaction.Id] = tcs;

                // Publicar o evento para o serviço de carteira processar
                var depositEvent = new DepositRequestedEvent(
                    transaction.Id,
                    walletId,
                    amount
                );

                _rabbitMQService.PublishDepositRequested(depositEvent);

                // Aguardar a confirmação com timeout
                var completionTask = tcs.Task;
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
                
                var completedTask = await Task.WhenAny(completionTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                {
                    _pendingTransactions.TryRemove(transaction.Id, out _);
                    return new ResultViewModel(false, "Timeout na operação de depósito", null);
                }

                return await completionTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "M65JH - Falha ao registrar depósito: {WalletId}, {Amount}", walletId, amount);
                return new ResultViewModel(false, "M65JH - Falha interna no servidor", null);
            }
        }

        public async Task<ResultViewModel> GetTransactionsAsync(Guid walletId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                IEnumerable<Models.Transaction> transactions;

                if (startDate.HasValue && endDate.HasValue)
                {
                    transactions = await _transactionRepository.GetByWalletIdAndDateRangeAsync(
                        walletId,
                        startDate.Value.ToUniversalTime(),
                        endDate.Value.ToUniversalTime()
                    );
                }
                else
                {
                    transactions = await _transactionRepository.GetByWalletIdAsync(walletId);
                }

                return new ResultViewModel(true, "Transações consultadas com sucesso", transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "H89LP - Falha ao consultar transações: {WalletId}", walletId);
                return new ResultViewModel(false, "H89LP - Falha interna no servidor", null);
            }
        }

        private async Task HandleBalanceUpdated(BalanceUpdatedEvent @event)
        {
            _logger.LogInformation("Evento BalanceUpdated recebido: {TransactionId}, {Success}", @event.TransactionId, @event.Success);

            if (_pendingTransactions.TryRemove(@event.TransactionId, out var tcs))
            {
                if (@event.Success)
                {
                    // Publicar confirmação de transação concluída
                    var completedEvent = new TransactionCompletedEvent(
                        @event.TransactionId,
                        true,
                        "Transação processada com sucesso"
                    );
                    _rabbitMQService.PublishTransactionCompleted(completedEvent);

                    tcs.SetResult(new ResultViewModel(
                        true,
                        "Transação processada com sucesso",
                        new { TransactionId = @event.TransactionId, NewBalance = @event.NewBalance }
                    ));
                }
                else
                {
                    // Publicar confirmação de transação falha
                    var completedEvent = new TransactionCompletedEvent(
                        @event.TransactionId,
                        false,
                        @event.Message
                    );
                    _rabbitMQService.PublishTransactionCompleted(completedEvent);

                    tcs.SetResult(new ResultViewModel(
                        false,
                        @event.Message,
                        null
                    ));
                }
            }
            else
            {
                _logger.LogWarning("Transação não encontrada: {TransactionId}", @event.TransactionId);
            }

            await Task.CompletedTask;
        }
    }
} 