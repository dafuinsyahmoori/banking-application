using BankingApplication.Entities;
using BankingApplication.Entities.Enums;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BankingApplication.DependencyRegistrations
{
    public static class PendingDepositCodes
    {
        public static IServiceCollection AddPendingDepositCodes(this IServiceCollection services)
        {
            return services.AddKeyedSingleton("depositCodes", (serviceProvider, _) =>
            {
                var collection = serviceProvider.GetRequiredService<IMongoCollection<Deposit>>();

                var deposits = collection.AsQueryable()
                    .Where(d => d.Status == DepositStatus.Pending)
                    .ToArray();

                foreach (var deposit in deposits)
                {
                    var delay = deposit.Due - DateTime.UtcNow;

                    if (delay.TotalSeconds <= 0)
                    {
                        var depositFilter = Builders<Deposit>.Filter.Eq(d => d.Code, deposit.Code);
                        var depositUpdate = Builders<Deposit>.Update.Set(d => d.Status, DepositStatus.Expired);

                        collection.UpdateOne(depositFilter, depositUpdate);

                        deposit.Status = DepositStatus.Expired;
                    }
                }

                var depositCodes = deposits.Where(d => d.Status == DepositStatus.Pending)
                    .ToDictionary(
                        d => d.Code!,
                        d => Task.Delay(d.Due - DateTime.UtcNow)
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