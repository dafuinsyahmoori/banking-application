using BankingApplication.Entities;
using BankingApplication.Entities.Enums;
using MongoDB.Driver;

namespace BankingApplication.DependencyRegistrations
{
    public static class PendingDepositCodes
    {
        public static IServiceCollection AddPendingDepositCodes(this IServiceCollection services)
        {
            return services.AddKeyedSingleton("depositCodes", (serviceProvider, _) =>
            {
                var collection = serviceProvider.GetRequiredService<IMongoCollection<Deposit>>();

                var depositCodes = collection.AsQueryable()
                    .Where(d => d.Status == DepositStatus.Pending)
                    .ToDictionary(
                        d => d.Code!,
                        d =>
                        {
                            var delay = d.Due - DateTime.UtcNow;

                            return Task.Delay(delay);
                        }
                    );

                foreach (var code in depositCodes.Keys)
                {
                    depositCodes[code] = depositCodes[code].ContinueWith(_ =>
                    {
                        var depositFilter = Builders<Deposit>.Filter.Eq(d => d.Code, code);
                        var depositUpdate = Builders<Deposit>.Update.Set(d => d.Status, DepositStatus.Expired);

                        collection.UpdateOne(depositFilter, depositUpdate);
                        depositCodes.Remove(code);

                        Console.WriteLine($"Withdrawal code {code} has just been expired");
                    });
                }

                return depositCodes;
            });
        }
    }
}