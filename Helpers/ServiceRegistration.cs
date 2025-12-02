using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using Serilog;
using Serilog.Exceptions;
using SharpAutomation.API;
using SharpAutomation.Config;

namespace SharpAutomation.Helpers
{
    public static class ServiceRegistration
    {
        public static ServiceProvider RegisterServices(string environmentName = null!)
        {
            try
            {
                DotNetEnv.Env.Load();

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

                services.Configure<BrowserSettings>(configuration.GetSection("BrowserSettings"));
                services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));

                services.AddSingleton<IConfiguration>(configuration);
                services.AddSingleton<ConfigurationHelper>();

                services.AddLogging(builder => builder.AddSerilog());


                services.AddSingleton<DriverFactory>();
                services.AddSingleton<AutoFixtureBuilder>();


                services.AddScoped(_ => CorrelationContextAccessor.Current);
                services.AddScoped<IWebDriver>(provider =>
                {
                    var driverFactory = provider.GetRequiredService<DriverFactory>();
                    var browserSettings = provider.GetRequiredService<IOptions<BrowserSettings>>().Value;

                    return driverFactory.CreateWebDriver(browserSettings.Browser);
                });

                services.AddScoped<CorrelationContext>();
                services.AddScoped<HttpClientDelegatingHandler>();

                services.AddHttpClient("ApiClient", (provider, client) =>
                {  
                    var apiSettings = provider.GetRequiredService<IOptions<ApiSettings>>().Value;
                    client.BaseAddress = new Uri(apiSettings.BaseUrl);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(apiSettings.ContentType));
                    client.Timeout = TimeSpan.FromSeconds(apiSettings.TimeoutSeconds);
                    client.DefaultRequestHeaders.Accept.Clear();
                })
                .AddHttpMessageHandler<HttpClientDelegatingHandler>();

                Log.Information("Service registration completed successfully.");
                return services.BuildServiceProvider();

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error during service registration.");
                throw;
            }
        }
    }
}
