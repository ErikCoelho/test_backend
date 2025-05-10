
namespace DigiPay.Wallet.Api.Models;
    public class Wallet
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public decimal Balance { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public Wallet(Guid userId)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Balance = 0;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // For EF Core
        protected Wallet() { }

        public void AddFunds(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");

            Balance += amount;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool DeductFunds(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");

            if (Balance < amount)
                return false;

            Balance -= amount;
            UpdatedAt = DateTime.UtcNow;
            return true;
        }
    }