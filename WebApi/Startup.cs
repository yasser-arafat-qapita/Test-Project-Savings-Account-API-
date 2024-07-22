using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Infrastructure.Extensions;
using Infrastructure.Repository;
using MongoDB.Driver;
using Projections.Handlers;
using Projections.Repositories;
using Projections.Services;
using SavingsAccountAPI.Filters;

namespace SavingsAccountAPI;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Configure MongoDB 
        var mongoSettings = _configuration.GetSection("MongoDbSettings");
        var mongoClient = new MongoClient(mongoSettings["ConnectionString"]);
        services.AddEventStore(_configuration);
        services.AddSingleton<IMongoClient>(s => mongoClient);
        services.AddHangfire(config =>
        {
            config.UseMongoStorage(mongoClient, mongoSettings["HangfireDb"], new MongoStorageOptions
            {
                MigrationOptions = new MongoMigrationOptions
                {
                    MigrationStrategy = new MigrateMongoMigrationStrategy(),
                    BackupStrategy = new CollectionMongoBackupStrategy()
                }
            });
        });
        services.AddHangfireServer(options =>
        {
            options.Queues = new[] { "critical", "default", "low" };
        });
        services.AddSingleton<CreateAccountStatementFilter>();
        services.AddHostedService<SavingsAccountProjectionService>();
        services.AddSingleton<SavingsAccountRepository>();
        services.AddSingleton<MongoProjectionHandler>();
        services.AddSingleton<MongoRepository>();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IHostApplicationLifetime lifetime, ILogger<Startup> logger)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseRouting();
        app.UseHangfireDashboard();
        app.UseHangfireServer();
        var serviceProvider = app.ApplicationServices;
        var customJobFilter = serviceProvider.GetRequiredService<CreateAccountStatementFilter>();
        GlobalJobFilters.Filters.Add(customJobFilter);
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}