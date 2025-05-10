using Microsoft.EntityFrameworkCore;

namespace DigiPay.Wallet.Api.Data
{
    public static class DatabaseMigrator
    {
        public static WebApplication MigrateDatabase(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                try
                {
                    app.Logger.LogInformation("Iniciando migração do banco de dados...");
                    dbContext.Database.Migrate();
                    app.Logger.LogInformation("Migração do banco de dados concluída com sucesso!");
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "Ocorreu um erro ao aplicar as migrações ao banco de dados.");
                    throw;
                }
            }

            return app;
        }
    }
} 