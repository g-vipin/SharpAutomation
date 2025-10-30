using Microsoft.Extensions.Logging;
using Serilog.Context;
using SharpAutomation.Helpers;

namespace SharpAutomation.API;
public class HttpClientDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<HttpClientDelegatingHandler> _logger;
    private readonly CorrelationContext _correlationContext;
    public HttpClientDelegatingHandler(ILogger<HttpClientDelegatingHandler> logger, CorrelationContext correlationContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _correlationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            if (!request.Headers.Contains("X-Correlation-ID"))
            {
                var correlationId = _correlationContext?.CorrelationId;
                request.Headers.Add("X-Correlation-ID", correlationId);
                _logger.LogDebug("Sending request with Correlation ID: {CorrelationID}", correlationId);
            }
            using (LogContext.PushProperty("CorrelationID", request.Headers.GetValues("X-Correlation-ID").FirstOrDefault()))
            {


                var startTime = DateTime.UtcNow;
                _logger.LogDebug("Initiating Api request for the url: '{request.RequestUri}' with method: '{request.Method}'", request.RequestUri, request.Method);

                if (request.Content != null)
                {
                    var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogDebug("Request body: '{RequestBody}'", requestBody);
                }


                var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                var stopTime = DateTime.UtcNow;
                var duration = stopTime - startTime;

                _logger.LogDebug("Response received. Success: {IsSuccess}, Status Code: {StatusCode}, URL: {RequestUri}, Duration: {Duration}ms",
                 response.IsSuccessStatusCode, response.StatusCode, request.RequestUri, duration.TotalMilliseconds);
                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogDebug("Response body for failed request: {ResponseBody}", responseBody);
                }
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending a request to {RequestUri} with method {Method}", request.RequestUri, request.Method);
            throw;
        }
    }
}