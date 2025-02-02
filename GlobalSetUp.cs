using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Internal;
using Serilog;
using SharpAutomation.Helpers;

namespace SharpAutomation;

[SetUpFixture]
public class GlobalSetUp
{
    public static ServiceProvider ServiceProvider;

    public static ConfigurationHelper ConfigurationHelper;

    public static Serilog.ILogger Logger = Log.ForContext<GlobalSetUp>();

    [OneTimeSetUp]
    public void Setup()
    {
        try
        {
            Logger.Information("Starting Global Setup..."); ;
            ServiceProvider = ServiceRegistration.RegisterServices();

            if (ServiceProvider == null)
            {
                Logger.Error("ServiceProvider is null. Initialization failed.");
                throw new InvalidOperationException("ServiceProvider initialization failed.");
            }
            Logger.Information("ServiceProvider initialized successfully.");

            var driverFactory = ServiceProvider.GetRequiredService<DriverFactory>();
            ConfigurationHelper = ServiceProvider.GetRequiredService<ConfigurationHelper>();
        }
        catch (Exception ex)
        {
            Logger.Fatal(ex, "Error during Global Setup.");
            throw;
        }
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        try
        {
            Logger.Information("Starting Global Teardown...");

            (ServiceProvider as IDisposable)?.Dispose();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error during Global tear down.");
            throw;
        }

    }

    public static T GetService<T>() where T : class
    {
        if (ServiceProvider == null)
            throw new InvalidOperationException("ServiceProvider is not initialized.");
        return ServiceProvider.GetRequiredService<T>();
    }
}
