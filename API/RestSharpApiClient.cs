using Microsoft.Extensions.Options;
using RestSharp;
using SharpAutomation.Config;

namespace SharpAutomation.API;

public class RestSharpApiClient : IRestSharpApiClient
{
    private readonly RestClient _client;

    public RestSharpApiClient(IOptions<ApiSettings> apiOptions)
    {
        ArgumentNullException.ThrowIfNull(apiOptions);

        var apiSettings = apiOptions.Value;
        var options = new RestClientOptions(apiSettings.BaseUrl)
        {
            Timeout = TimeSpan.FromSeconds(apiSettings.ResolvedTimeoutSeconds)
        };

        _client = new RestClient(options);
        _client.AddDefaultHeader("Accept", apiSettings.ContentType);
    }

    public Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default)
    {
        return _client.ExecuteAsync(request, cancellationToken);
    }

    public Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request, CancellationToken cancellationToken = default)
    {
        return _client.ExecuteAsync<T>(request, cancellationToken);
    }
}
