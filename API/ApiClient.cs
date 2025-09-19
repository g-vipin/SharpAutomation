using System.Text.Json;
using OpenQA.Selenium;
using Polly;

namespace SharpAutomation.API;

public abstract class ApiClient
{
    protected readonly HttpClient Client;
    private readonly IAsyncPolicy<HttpResponseMessage> _policy;
    private readonly JsonSerializerOptions _jsonOptions;

    protected ApiClient()
    {
        Client = HttpClientFactory.GetClient();
        _policy = HttpClientFactory.GetPolicy();
        _jsonOptions = HttpClientFactory.GetJsonOptions();
    }

    protected async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await _policy.ExecuteAsync(() =>
            Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
        );
    }

    protected async Task<T?> SendAndDeserializeAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var response = await _policy.ExecuteAsync(
            async () => await Client.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, cancellationToken
            )
        );

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken);
    }

}