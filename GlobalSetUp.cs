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
            ConfigurationHelper = ServiceProvider.GetRequiredService<ConfigurationHelper>();

            if (ServiceProvider == null)
            {
                Logger.Error("ServiceProvider is null. Initialization failed.");
                throw new InvalidOperationException("ServiceProvider initialization failed.");
            }
            Logger.Information("ServiceProvider initialized successfully.");
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

    public static IServiceScope CreateTestScope()
    {
        return ServiceProvider.CreateScope();
    }

    public static T GetService<T>() where T : class
    {
        if (ServiceProvider == null)
        {
            Logger.Error("ServiceProvider is not initialized.");
            throw new InvalidOperationException("ServiceProvider is not initialized.");
        }
        Logger.Information($"Resolving service: {typeof(T).Name}");
        var service = ServiceProvider.GetRequiredService<T>();
        Logger.Information($"Service {typeof(T).Name} resolved successfully.");
        return service;
    }

}
