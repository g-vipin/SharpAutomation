using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;
using SharpAutomation.Helpers;

namespace SharpAutomation
{
    [SetUpFixture]
    public class GlobalSetUp
    {
        public static ServiceProvider ServiceProvider;

        [OneTimeSetUp]
        public void Setup()
        {
            try
            {
                Log.Information("Starting Global Setup...");
                
                // Build the DI container
                ServiceProvider = ServiceRegistration.RegisterServices();

                if (ServiceProvider == null)
                {
                    Log.Error("ServiceProvider is null. Initialization failed.");
                    throw new InvalidOperationException("ServiceProvider initialization failed.");
                }

                Log.Information("ServiceProvider initialized successfully.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error during Global Setup.");
                throw;
            }
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            try
            {
                Log.Information("Starting Global Teardown...");
                (ServiceProvider as IDisposable)?.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during Global tear down.");
                throw;
            }
        }

        // Create a new scope for each test
        public static IServiceScope CreateTestScope()
        {
            return ServiceProvider.CreateScope();
        }

        // Utility: resolve services anywhere
        public static T GetService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }
    }
}
