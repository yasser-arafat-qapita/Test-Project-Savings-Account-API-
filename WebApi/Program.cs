using NServiceBus;
using Serilog;
using Serilog.Events;

namespace SavingsAccountAPI;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            // NServiceBus Configuration
            var endpointConfiguration = new EndpointConfiguration("SavingsAccountBus");
            endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
            var transport = endpointConfiguration.UseTransport<LearningTransport>();
            
            string username = Environment.UserName;
            string storageDirectory = $"/home/{username}/ldt";
            transport.StorageDirectory(storageDirectory);
            
            Log.Information("Starting up the web host");
            
            var hostBuilder = CreateHostBuilder(args);
            hostBuilder.UseNServiceBus(context => endpointConfiguration);
            hostBuilder.Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
}