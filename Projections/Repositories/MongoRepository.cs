using Domain.Aggregates;
using Infrastructure.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Projections.View;

namespace Projections.Repositories;

public class MongoRepository
{
    private readonly IMongoCollection<SavingsAccountView> _collection;
    private readonly SavingsAccountRepository _savingsAccountRepository;
    private readonly ILogger<MongoRepository> _logger;

    public MongoRepository(IMongoClient mongoClient, SavingsAccountRepository savingsAccountRepository, 
        ILogger<MongoRepository> logger, IConfiguration configuration)
    {
        _logger = logger;
        _savingsAccountRepository = savingsAccountRepository;
        var database = mongoClient.GetDatabase(configuration.GetSection("MongoDbSettings:DatabaseName").Value);
        _collection = database.GetCollection<SavingsAccountView>(configuration.GetSection("MongoDbSettings:Collection").Value);
    }

    public async Task SaveAsync(string accountId)
    {
        var latestAggregate = await _savingsAccountRepository.GetByIdAsync(accountId);
        var savingsAccountView = GetSavingsAccountView(latestAggregate);
        await _collection.ReplaceOneAsync(a => a.BankAccountId == savingsAccountView.BankAccountId, savingsAccountView, new ReplaceOptions { IsUpsert = true });
    }

    private SavingsAccountView GetSavingsAccountView(SavingsAccountAggregate aggregate)
    {
        return new SavingsAccountView
        {
            BankAccountId = aggregate.AccountId,
            Balance = aggregate.Balance
        };
    }
}