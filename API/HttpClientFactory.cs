using System.Net.Http.Headers;
using System.Text.Json;

namespace SharpAutomation.API;

public static class HttpClientFactory
{
    private static readonly HttpClient _client;

    private static readonly JsonSerializerOptions _JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,

    };

    static HttpClientFactory()
    {
        _client = new HttpClient
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    }

    public static HttpClient GetClient() => _client;
    public static JsonSerializerOptions GetJsonOptions() => _JsonOptions;
}