using DigiPay.Transaction.Api.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigiPay.Transaction.Api.Repositories.Contracts
{
    public interface ITransactionRepository
    {
        Task AddAsync(Models.Transaction transaction);
        Task<IEnumerable<Models.Transaction>> GetByWalletIdAsync(Guid walletId);
        Task<IEnumerable<Models.Transaction>> GetByWalletIdAndDateRangeAsync(Guid walletId, DateTime startDate, DateTime endDate);
    }
} 