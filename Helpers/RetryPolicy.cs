using System.Diagnostics;
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

     public static async Task<TResult> RetryAsync<TResult>(
        Func<object[], Task<TResult>> action,
        Predicate<TResult> condition,
        TimeSpan timeout,
        TimeSpan delay,
        int retryCount,
        Func<Exception, bool>? exceptionFilter = null,
        params object[] args)
    {
        Exception? lastException = null;
        var sw = Stopwatch.StartNew();
        var attemptDelay = delay;

        for (int attempt = 1; attempt <= retryCount; attempt++)
        {
            try
            {
                var result = await action(args);

                if (condition(result))
                    return result;
            }
            catch (Exception ex)
            {
                lastException = ex;

                if (exceptionFilter != null && !exceptionFilter(ex))
                    throw;

                if (sw.Elapsed >= timeout)
                    throw new TimeoutException($"Retry timeout of {timeout} exceeded.", ex);

                Log.Logger.Warning("Attempt {Attempt}/{RetryCount} failed. Error: {Message}. Retrying...",
                    attempt, retryCount, ex.Message);
            }

            if (attempt < retryCount)
            {
                await Task.Delay(attemptDelay);

                attemptDelay = TimeSpan.FromMilliseconds(attemptDelay.TotalMilliseconds * 1.5);
            }
        }

        throw new InvalidOperationException(
            $"Retry attempts ({retryCount}) exhausted without meeting condition.",
            lastException
        );
    }

}