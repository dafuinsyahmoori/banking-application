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

                var withdrawals = collection.AsQueryable()
                    .Where(w => w.Status == WithdrawalStatus.Pending)
                    .ToArray();

                foreach (var withdrawal in withdrawals)
                {
                    var delay = withdrawal.Due - DateTime.UtcNow;

                    if (delay.TotalSeconds <= 0)
                    {
                        var depositFilter = Builders<Withdrawal>.Filter.Eq(w => w.Code, withdrawal.Code);
                        var depositUpdate = Builders<Withdrawal>.Update.Set(w => w.Status, WithdrawalStatus.Expired);

                        collection.UpdateOne(depositFilter, depositUpdate);

                        withdrawal.Status = WithdrawalStatus.Expired;
                    }
                }

                var withdrawalCodes = withdrawals.Where(w => w.Status == WithdrawalStatus.Pending)
                    .ToDictionary(
                        d => d.Code!,
                        d => Task.Delay(d.Due - DateTime.UtcNow)
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