using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SharpAutomation.Config;

namespace SharpAutomation.API;

public static class ServiceCollectionApiExtensions
{
    public static IServiceCollection AddSharpApiClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ApiResiliencePolicyFactory>();

        services.AddHttpClient<IModularMonolithApiClient, ModularMonolithApiClient>((provider, client) =>
            {
                var apiSettings = provider.GetRequiredService<IOptions<ApiSettings>>().Value;
                ConfigureClient(client, apiSettings);
            })
            .AddHttpMessageHandler(provider =>
            {
                var apiSettings = provider.GetRequiredService<IOptions<ApiSettings>>().Value;
                return new ApiAuthenticationDelegatingHandler(apiSettings.Auth);
            })
            .AddHttpMessageHandler<HttpClientDelegatingHandler>();

        services.AddScoped<IHttpApiClient>(provider =>
            provider.GetRequiredService<IModularMonolithApiClient>());

        var microservices = configuration.GetSection("ApiSettings:Microservices")
            .Get<Dictionary<string, ApiEndpointSettings>>() ?? [];

        foreach (var microservice in microservices)
        {
            services.AddNamedHttpClient(microservice.Key, microservice.Value);
        }

        services.AddTransient<HttpClientDelegatingHandler>();
        services.AddScoped<IMicroserviceApiClientFactory, MicroserviceApiClientFactory>();
        services.AddScoped<IRestSharpApiClient, RestSharpApiClient>();

        return services;
    }

    private static IHttpClientBuilder AddNamedHttpClient(
        this IServiceCollection services,
        string clientName,
        ApiEndpointSettings endpointSettings)
    {
        return services.AddHttpClient(clientName, client => ConfigureClient(client, endpointSettings))
            .AddHttpMessageHandler(() => new ApiAuthenticationDelegatingHandler(endpointSettings.Auth))
            .AddHttpMessageHandler<HttpClientDelegatingHandler>();
    }

    private static void ConfigureClient(HttpClient client, ApiEndpointSettings settings)
    {
        client.BaseAddress = new Uri(settings.BaseUrl);
        client.Timeout = Timeout.InfiniteTimeSpan;
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(settings.ContentType));
    }
}
