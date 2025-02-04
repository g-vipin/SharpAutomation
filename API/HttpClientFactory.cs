
using System.Net.Http.Headers;

namespace SharpAutomation.API;
public static class HttpClientFactory
{
    private readonly static HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(10)};

    static HttpClientFactory()
    {
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public static HttpClient GetClient()
    {
        return _client;
    }
}