using System.Net;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Serilog;

public static class RetryPolicy
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount = 6)
    {
        var sleepDuration = Backoff.ExponentialBackoff(
            initialDelay: TimeSpan.FromSeconds(2),
            retryCount: retryCount,
            factor: 2
        );

        return Policy
        .Handle<HttpRequestException>()
        .Or<TaskCanceledException>()
        .OrResult<HttpResponseMessage>(r =>
        (int)r.StatusCode >= 500 || r.StatusCode == HttpStatusCode.RequestTimeout
        )
        .WaitAndRetryAsync(
            sleepDurations: sleepDuration,
            onRetry: (outcome, timespan, retryAttempt, context) =>
            {
                var result = outcome.Exception is not null ?
                 outcome.Exception?.Message : outcome.Result.StatusCode.ToString();
                Log.Logger.Debug($"Http Retry Attempt: {retryAttempt} after {timespan.TotalSeconds}s with {result}");
            }

        );
    }
}