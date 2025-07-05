using BankingApplication.Entities;
using BankingApplication.Entities.Enums;
using MongoDB.Driver;

namespace BankingApplication.DependencyRegistrations
{
    public static class PendingWithdrawalCodes
    {
        public static IServiceCollection AddPendingWithdrawalCodes(this IServiceCollection services)
        {
            return services.AddKeyedSingleton("withdrawalCodes", (serviceProvider, _) =>
            {
                var collection = serviceProvider.GetRequiredService<IMongoCollection<Withdrawal>>();

                var withdrawalCodes = collection.AsQueryable()
                    .Where(w => w.Status == WithdrawalStatus.Pending)
                    .ToDictionary(
                        w => w.Code!,
                        w =>
                        {
                            var delay = w.Due - DateTime.UtcNow;

                            return Task.Delay(delay);
                        }
                    );

                foreach (var code in withdrawalCodes.Keys)
                {
                    withdrawalCodes[code] = withdrawalCodes[code].ContinueWith(_ =>
                    {
                        var withdrawalFilter = Builders<Withdrawal>.Filter.Eq(w => w.Code, code);
                        var withdrawalUpdate = Builders<Withdrawal>.Update.Set(w => w.Status, WithdrawalStatus.Expired);

                        collection.UpdateOne(withdrawalFilter, withdrawalUpdate);
                        withdrawalCodes.Remove(code);

                        Console.WriteLine($"Withdrawal code {code} has just been expired");
                    });
                }

                return withdrawalCodes;
            });
        }
    }
}