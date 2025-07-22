using BankingApplication.Entities;
using MongoDB.Driver;

namespace BankingApplication.DependencyRegistrations
{
    public static class DatabaseCollections
    {
        public static IServiceCollection AddDatabaseCollections(this IServiceCollection services)
        {
            return services
                .AddSingleton(serviceProvider =>
                {
                    var database = serviceProvider.GetRequiredService<IMongoDatabase>();
                    var collection = database.GetCollection<User>("users");

                    var existingIndexes = collection.Indexes.List();

                    var doesIndexExist = existingIndexes
                        .ToList()
                        .Any(
                            index => index.Any(
                                el => el.Name == "name" && el.Value.IsString && el.Value.AsString.Contains("email") && el.Value.AsString.Contains("username")
                            )
                        );

                    if (doesIndexExist)
                        return collection;

                    var indexKey = Builders<User>.IndexKeys
                        .Ascending(u => u.Email)
                        .Ascending(u => u.Username);

                    var indexModel = new CreateIndexModel<User>(indexKey, new() { Unique = true });

                    collection.Indexes.CreateOne(indexModel);

                    return collection;
                })
                .AddSingleton(serviceProvider =>
                {
                    var database = serviceProvider.GetRequiredService<IMongoDatabase>();
                    var collection = database.GetCollection<Account>("accounts");

                    var existingIndexes = collection.Indexes.List();

                    var doesIndexExist = existingIndexes
                        .ToList()
                        .Any(
                            index => index.Any(
                                el => el.Name == "name" && el.Value.IsString && el.Value.AsString.Contains("number")
                            )
                        );

                    if (doesIndexExist)
                        return collection;

                    var indexKey = Builders<Account>.IndexKeys.Ascending(a => a.Number);

                    var indexModel = new CreateIndexModel<Account>(indexKey, new() { Unique = true });

                    collection.Indexes.CreateOne(indexModel);

                    return collection;
                })
                .AddSingleton(serviceProvider =>
                {
                    var database = serviceProvider.GetRequiredService<IMongoDatabase>();
                    var collection = database.GetCollection<Withdrawal>("withdrawals");

                    var existingIndexes = collection.Indexes.List();

                    var doesIndexExist = existingIndexes
                        .ToList()
                        .Any(
                            index => index.Any(
                                el => el.Name == "name" && el.Value.IsString && el.Value.AsString.Contains("code")
                            )
                        );

                    if (doesIndexExist)
                        return collection;

                    var indexKey = Builders<Withdrawal>.IndexKeys.Ascending(w => w.Code);

                    var indexModel = new CreateIndexModel<Withdrawal>(indexKey, new() { Unique = true });

                    collection.Indexes.CreateOne(indexModel);

                    return collection;
                })
                .AddSingleton(serviceProvider =>
                {
                    var database = serviceProvider.GetRequiredService<IMongoDatabase>();
                    var collection = database.GetCollection<Deposit>("deposits");

                    var existingIndexes = collection.Indexes.List();

                    var doesIndexExist = existingIndexes
                        .ToList()
                        .Any(
                            index => index.Any(
                                el => el.Name == "name" && el.Value.IsString && el.Value.AsString.Contains("code")
                            )
                        );

                    if (doesIndexExist)
                        return collection;

                    var indexKey = Builders<Deposit>.IndexKeys.Ascending(d => d.Code);

                    var indexModel = new CreateIndexModel<Deposit>(indexKey, new() { Unique = true });

                    collection.Indexes.CreateOne(indexModel);

                    return collection;
                })
                .AddSingleton(serviceProvider =>
                {
                    var database = serviceProvider.GetRequiredService<IMongoDatabase>();
                    return database.GetCollection<TransactionHistory>("transactionHistories");
                })
                .AddSingleton(serviceProvider =>
                {
                    var database = serviceProvider.GetRequiredService<IMongoDatabase>();
                    var collection = database.GetCollection<Admin>("admins");

                    var existingIndexes = collection.Indexes.List();

                    var doesIndexExist = existingIndexes
                        .ToList()
                        .Any(
                            index => index.Any(
                                el => el.Name == "name" && el.Value.IsString && el.Value.AsString.Contains("email")
                            )
                        );

                    if (doesIndexExist)
                        return collection;

                    var indexKey = Builders<Admin>.IndexKeys.Ascending(a => a.Email);

                    var indexModel = new CreateIndexModel<Admin>(indexKey, new() { Unique = true });

                    collection.Indexes.CreateOne(indexModel);

                    return collection;
                })
                .AddSingleton(serviceProvider =>
                {
                    var database = serviceProvider.GetRequiredService<IMongoDatabase>();
                    return database.GetCollection<ComplaintRequest>("complaintRequests");
                })
                .AddSingleton(serviceProvider =>
                {
                    var database = serviceProvider.GetRequiredService<IMongoDatabase>();
                    var collection = database.GetCollection<ComplaintResponse>("complaintResponses");

                    var existingIndexes = collection.Indexes.List();

                    var doesIndexExist = existingIndexes
                        .ToList()
                        .Any(
                            index => index.Any(
                                el => el.Name == "name" && el.Value.IsString && el.Value.AsString.Contains("complaintRequestId")
                            )
                        );

                    if (doesIndexExist)
                        return collection;

                    var indexKey = Builders<ComplaintResponse>.IndexKeys.Ascending(cr => cr.ComplaintRequestId);

                    var indexModel = new CreateIndexModel<ComplaintResponse>(indexKey, new() { Unique = true });

                    collection.Indexes.CreateOne(indexModel);

                    return collection;
                });
        }
    }
}