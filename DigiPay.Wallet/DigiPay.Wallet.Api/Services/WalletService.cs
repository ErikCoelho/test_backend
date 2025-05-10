using DigiPay.Wallet.Api.Models;
using DigiPay.Wallet.Api.Repositories.Contracts;
using DigiPay.Wallet.Api.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigiPay.Wallet.Api.Services
{
    public class WalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogger<WalletService> _logger;

        public WalletService(
            IWalletRepository walletRepository, 
            ITransactionRepository transactionRepository,
            ILogger<WalletService> logger)
        {
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        public async Task<ResultViewModel> GetBalanceAsync(Guid userId)
        {
            try
            {
                var wallet = await GetOrCreateWalletAsync(userId);
                return new ResultViewModel(true, "Saldo consultado com sucesso", new { Balance = wallet.Balance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "9P32F - Falha ao consultar saldo: {UserId}", userId);
                return new ResultViewModel(false, "9P32F - Falha interna no servidor", null);
            }
        }

        public async Task<ResultViewModel> AddFundsAsync(Guid userId, decimal amount)
        {
            try
            {
                if (amount <= 0)
                {
                    return new ResultViewModel(false, "O valor deve ser maior que zero", null);
                }

                var wallet = await GetOrCreateWalletAsync(userId);
                wallet.AddFunds(amount);

                await _walletRepository.UpdateAsync(wallet);

                // Record the transaction
                var transaction = new Transaction(
                    wallet.Id,
                    amount,
                    TransactionType.Deposit,
                    "Depósito de fundos"
                );

                await _transactionRepository.AddAsync(transaction);

                return new ResultViewModel(true, "Depósito realizado com sucesso", new { NewBalance = wallet.Balance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "M65JH - Falha ao adicionar fundos: {UserId}, {Amount}", userId, amount);
                return new ResultViewModel(false, "M65JH - Falha interna no servidor", null);
            }
        }

        public async Task<ResultViewModel> TransferFundsAsync(
            Guid fromUserId, 
            Guid toUserId, 
            decimal amount, 
            string description)
        {
            try
            {
                if (fromUserId == toUserId)
                {
                    return new ResultViewModel(false, "Não é possível transferir para a mesma carteira", null);
                }

                if (amount <= 0)
                {
                    return new ResultViewModel(false, "O valor deve ser maior que zero", null);
                }

                var sourceWallet = await GetOrCreateWalletAsync(fromUserId);
                var destinationWallet = await GetOrCreateWalletAsync(toUserId);

                if (sourceWallet.Balance < amount)
                {
                    return new ResultViewModel(false, "Saldo insuficiente para realizar a transferência", null);
                }

                // Perform the transfer
                if (!sourceWallet.DeductFunds(amount))
                {
                    return new ResultViewModel(false, "Saldo insuficiente para realizar a transferência", null);
                }

                destinationWallet.AddFunds(amount);

                // Update both wallets
                await _walletRepository.UpdateAsync(sourceWallet);
                await _walletRepository.UpdateAsync(destinationWallet);

                // Record the transaction for sender
                var senderTransaction = new Transaction(
                    sourceWallet.Id,
                    amount,
                    TransactionType.Transfer,
                    description,
                    destinationWallet.Id
                );

                await _transactionRepository.AddAsync(senderTransaction);

                return new ResultViewModel(
                    true, 
                    "Transferência realizada com sucesso", 
                    new { NewBalance = sourceWallet.Balance }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "T47KP - Falha ao transferir fundos: {FromUserId}, {ToUserId}, {Amount}", 
                    fromUserId, toUserId, amount);
                return new ResultViewModel(false, "T47KP - Falha interna no servidor", null);
            }
        }

        public async Task<ResultViewModel> GetTransactionsAsync(Guid userId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var wallet = await GetOrCreateWalletAsync(userId);
                IEnumerable<Transaction> transactions;

                if (startDate.HasValue && endDate.HasValue)
                {
                    transactions = await _transactionRepository.GetByWalletIdAndDateRangeAsync(
                        wallet.Id, 
                        startDate.Value.ToUniversalTime(), 
                        endDate.Value.ToUniversalTime()
                    );
                }
                else
                {
                    transactions = await _transactionRepository.GetByWalletIdAsync(wallet.Id);
                }

                return new ResultViewModel(true, "Transações consultadas com sucesso", transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "H89LP - Falha ao consultar transações: {UserId}", userId);
                return new ResultViewModel(false, "H89LP - Falha interna no servidor", null);
            }
        }

        private async Task<Models.Wallet> GetOrCreateWalletAsync(Guid userId)
        {
            var wallet = await _walletRepository.GetByUserIdAsync(userId);
            
            if (wallet == null)
            {
                wallet = new Models.Wallet(userId);
                await _walletRepository.AddAsync(wallet);
            }
            
            return wallet;
        }
    }
} 