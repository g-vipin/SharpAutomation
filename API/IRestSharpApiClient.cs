using RestSharp;

namespace SharpAutomation.API;

public interface IRestSharpApiClient
{
    Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default);

    Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request, CancellationToken cancellationToken = default);
}
