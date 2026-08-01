namespace SharpAutomation.API;

public interface IHttpApiClient
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);

    Task<T?> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> SendJsonAsync<TRequest>(
        HttpMethod method,
        string requestUri,
        TRequest? body,
        CancellationToken cancellationToken = default);

    Task<TResponse?> SendJsonAsync<TRequest, TResponse>(
        HttpMethod method,
        string requestUri,
        TRequest? body,
        CancellationToken cancellationToken = default);

    Task<TResponse?> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken = default);
}
