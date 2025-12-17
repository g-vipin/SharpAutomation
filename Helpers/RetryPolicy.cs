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

    public static async Task<T> RetryAsync<T>(
        Func<CancellationToken, Task<T>> action,
        Predicate<T> condition,
        TimeSpan timeout,
        TimeSpan delay,
        int retryCount,
        Func<Exception, bool>? exceptionFilter = null)
    {
        using var cts = new CancellationTokenSource(timeout);
        Exception? lastException = null;

        for (int attempt = 1; attempt <= retryCount; attempt++)
        {
            try
            {
                var result = await action(cts.Token);

                if (condition(result))
                    return result;
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
                throw new TimeoutException($"The operation timed out after {timeout}.");
            }
            catch (Exception ex)
            {
                lastException = ex;

                if (exceptionFilter != null && !exceptionFilter(ex))
                    throw;
            }

            if (attempt < retryCount)
                await Task.Delay(delay, cts.Token);
        }

        throw new InvalidOperationException(
            $"Retry count {retryCount} exceeded and condition was never met.",
            lastException);
    }

}