using System.Text.Json;
using System.Net.Http.Json;

namespace SharpAutomation.API;
public class HttpApiClient : IHttpApiClient
{
    private readonly HttpClient _client;
    private readonly ApiResiliencePolicyFactory _policyFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpApiClient(HttpClient client, ApiResiliencePolicyFactory policyFactory)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _policyFactory = policyFactory ?? throw new ArgumentNullException(nameof(policyFactory));
        _jsonOptions = ApiJsonOptions.Default;
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var policy = _policyFactory.Create(cancellationToken);

        return await policy.ExecuteAsync(
            async ct => await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct)
                .ConfigureAwait(false),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<T?> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        if (response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<HttpResponseMessage> SendJsonAsync<TRequest>(
        HttpMethod method,
        string requestUri,
        TRequest? body,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateJsonRequest(method, requestUri, body);
        return await SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TResponse?> SendJsonAsync<TRequest, TResponse>(
        HttpMethod method,
        string requestUri,
        TRequest? body,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateJsonRequest(method, requestUri, body);
        return await SendAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TResponse?> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        return await SendAsync<TResponse>(request, cancellationToken).ConfigureAwait(false);
    }

    private HttpRequestMessage CreateJsonRequest<TRequest>(HttpMethod method, string requestUri, TRequest? body)
    {
        var request = new HttpRequestMessage(method, requestUri);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: _jsonOptions);
        }

        return request;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = response.Content is null
            ? null
            : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        throw new ApiRequestException(
            $"API request failed with status code {(int)response.StatusCode} ({response.ReasonPhrase}).",
            response.StatusCode,
            responseBody);
    }
}
