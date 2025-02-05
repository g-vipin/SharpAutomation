using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using Serilog;
using Serilog.Exceptions;

namespace SharpAutomation.Helpers
{
    public static class ServiceRegistration
    {
        public static ServiceProvider RegisterServices(string environmentName = null!)
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .MinimumLevel.Debug()
                    .Enrich.WithExceptionDetails()
                    .Enrich.FromLogContext()
                    .CreateLogger();

                Log.Information("Starting service registration...");

                var services = new ServiceCollection();

                services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());

                services.AddSingleton<IConfiguration>(configuration);
                services.AddSingleton<ConfigurationHelper>();
                services.AddSingleton<DriverFactory>();

                services.AddTransient<IWebDriver>(provider =>
                {
                    var configHelper = provider.GetRequiredService<ConfigurationHelper>();
                    var driverFactory = provider.GetRequiredService<DriverFactory>();
                    var browserConfig = configHelper.GetConfig("AppSettings:Browser");
                    return driverFactory.CreateWebDriver(browserConfig);
                });

                services.AddHttpClient("ApiClient", client =>
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.BaseAddress = new Uri("https://restful-booker.herokuapp.com/");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.Timeout = TimeSpan.FromSeconds(30);
                });

                Log.Information("Service registration completed successfully.");
                var serviceProvider = services.BuildServiceProvider();
                Log.Information("ServiceProvider created successfully.");
                return serviceProvider;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error during service registration.");
                throw;
            }
        }
    }
}
