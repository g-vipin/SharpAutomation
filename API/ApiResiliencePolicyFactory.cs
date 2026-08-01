using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using SharpAutomation.Config;

namespace SharpAutomation.API;

public sealed class ApiResiliencePolicyFactory
{
    private readonly ApiResilienceSettings _settings;
    private readonly ILogger<ApiResiliencePolicyFactory> _logger;

    public ApiResiliencePolicyFactory(IOptions<ApiSettings> apiOptions, ILogger<ApiResiliencePolicyFactory> logger)
    {
        ArgumentNullException.ThrowIfNull(apiOptions);

        _settings = apiOptions.Value.Resilience;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IAsyncPolicy<HttpResponseMessage> Create(CancellationToken cancellationToken)
    {
        var timeout = Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(_settings.TimeoutSeconds),
            TimeoutStrategy.Optimistic);

        var circuitBreaker = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<SocketException>()
            .Or<TimeoutRejectedException>()
            .Or<TaskCanceledException>(ex => !cancellationToken.IsCancellationRequested)
            .OrResult(response =>
                response.StatusCode == HttpStatusCode.RequestTimeout ||
                (int)response.StatusCode >= 500)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: _settings.CircuitBreakerFailuresBeforeBreak,
                durationOfBreak: TimeSpan.FromSeconds(_settings.CircuitBreakerBreakSeconds),
                onBreak: (outcome, duration) =>
                    _logger.LogWarning(
                        "API circuit opened for {DurationSeconds}s because of {Reason}.",
                        duration.TotalSeconds,
                        GetReason(outcome)),
                onReset: () => _logger.LogInformation("API circuit reset."),
                onHalfOpen: () => _logger.LogInformation("API circuit half-open."));

        var retry = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>(IsTransientRequestException)
            .Or<SocketException>()
            .Or<TimeoutRejectedException>()
            .Or<TaskCanceledException>(ex => !cancellationToken.IsCancellationRequested)
            .Or<BrokenCircuitException>()
            .OrResult(response =>
                response.StatusCode == HttpStatusCode.RequestTimeout ||
                (int)response.StatusCode >= 500)
            .WaitAndRetryAsync(
                retryCount: _settings.RetryCount,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromMilliseconds(_settings.RetryBaseDelayMilliseconds * Math.Pow(2, attempt - 1)),
                onRetry: (outcome, delay, attempt, _) =>
                {
                    _logger.LogWarning(
                        "API retry {Attempt}/{RetryCount} after {DelayMs}ms because of {Reason}.",
                        attempt,
                        _settings.RetryCount,
                        delay.TotalMilliseconds,
                        GetReason(outcome));
                });

        return Policy.WrapAsync(retry, circuitBreaker, timeout);
    }

    private static bool IsTransientRequestException(HttpRequestException exception)
    {
        return exception.InnerException is SocketException ||
               exception.StatusCode is null ||
               (int)exception.StatusCode >= 500 ||
               exception.StatusCode == HttpStatusCode.RequestTimeout;
    }

    private static string GetReason(DelegateResult<HttpResponseMessage> outcome)
    {
        if (outcome.Exception is not null)
        {
            return outcome.Exception.Message;
        }

        return $"{(int)outcome.Result.StatusCode} {outcome.Result.ReasonPhrase}";
    }
}
