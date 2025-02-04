using System.Text.Json;

namespace SharpAutomation.API;
public abstract class ApiClient
{
    protected HttpClient Client;

    protected ApiClient()
    {
        Client = HttpClientFactory.GetClient();
    }

    protected async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request)
    {
        return await Client.SendAsync(request);
    }

    protected async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content)!;
    }

}