
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Polly;

namespace SharpAutomation.API;

public static class HttpClientFactory
{
    private static readonly HttpClient _client;

    private static readonly JsonSerializerOptions _JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,

    };

    private static readonly IAsyncPolicy<HttpResponseMessage> _policy;

    static HttpClientFactory()
    {
        _client = new HttpClient
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _policy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult<HttpResponseMessage>(r =>
                (int)r.StatusCode >= 500 || r.StatusCode == HttpStatusCode.RequestTimeout
            )
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),

                            onRetry: (outcome, timespan, retryAttempt, context) =>
                        {
                            Console.WriteLine($"[HTTP RETRY] Attempt {retryAttempt} after {timespan.TotalSeconds}s: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
                        }
                    )
                    .WrapAsync(
                        Policy<HttpResponseMessage>
                            .Handle<Exception>()
                            .CircuitBreakerAsync(
                                handledEventsAllowedBeforeBreaking: 5,
                                durationOfBreak: TimeSpan.FromSeconds(30),
                                onBreak: (result, breakDelay) =>
                                {
                                    Console.WriteLine($"[HTTP CIRCUIT BREAK] Open for {breakDelay.TotalSeconds}s due to {result.Exception?.Message ?? result.Result.StatusCode.ToString()}");
                                },
                                onReset: () => Console.WriteLine("[HTTP CIRCUIT BREAK] Closed, calls will flow again."),
                                onHalfOpen: () => Console.WriteLine("[HTTP CIRCUIT BREAK] Half-open: trial call allowed.")
                        )
                        );
    }

    public static HttpClient GetClient() => _client;
    public static JsonSerializerOptions GetJsonOptions() => _JsonOptions;
    public static IAsyncPolicy<HttpResponseMessage> GetPolicy() => _policy;
}