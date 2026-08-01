using System.Net.Http;
using System.Text.Json;

namespace SharpAutomation.API;

public abstract class ApiClient
{
    protected readonly HttpClient Client;
    private readonly ApiResiliencePolicyFactory _policyFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    protected ApiClient(HttpClient client, ApiResiliencePolicyFactory policyFactory)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));
        _policyFactory = policyFactory ?? throw new ArgumentNullException(nameof(policyFactory));
        _jsonOptions = ApiJsonOptions.Default;
    }

    protected async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _policyFactory.Create(cancellationToken).ExecuteAsync(
            async ct => await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct)
                .ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);
    }

    protected async Task<T?> SendAndDeserializeAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken).ConfigureAwait(false);
    }

}
