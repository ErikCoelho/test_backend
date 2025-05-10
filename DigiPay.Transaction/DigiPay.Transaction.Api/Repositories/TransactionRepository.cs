using DigiPay.Transaction.Api.Models;
using DigiPay.Transaction.Api.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigiPay.Transaction.Api.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly DbContext _context;

        public TransactionRepository(DbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Models.Transaction transaction)
        {
            await _context.Set<Models.Transaction>().AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Models.Transaction>> GetByWalletIdAsync(Guid walletId)
        {
            return await _context.Set<Models.Transaction>()
                .Where(t => t.WalletId == walletId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Models.Transaction>> GetByWalletIdAndDateRangeAsync(Guid walletId, DateTime startDate, DateTime endDate)
        {
            return await _context.Set<Models.Transaction>()
                .Where(t => t.WalletId == walletId && 
                           t.CreatedAt >= startDate && 
                           t.CreatedAt <= endDate)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }
} 