using DigiPay.Auth.Api.Models;
using System.Security.Cryptography;
using System.Text;

namespace DigiPay.Auth.Api.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return; 
            }

            var users = new List<User>
            {
                new("admin", "admin@digipay.com", HashPassword("admin123")),
                new("user1", "user1@digipay.com", HashPassword("user123")),
                new("user2", "user2@digipay.com", HashPassword("user123"))
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
} 