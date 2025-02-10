using Microsoft.Extensions.Logging;

namespace SharpAutomation.API;
public class HttpClientDelegatingHandler : DelegatingHandler
{
    private readonly ILogger<HttpClientDelegatingHandler> _logger;
    public HttpClientDelegatingHandler(ILogger<HttpClientDelegatingHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            if (!request.Headers.Contains("X-Correlation-ID"))
            {
                request.Headers.Add("X-Correlation-ID", Guid.NewGuid().ToString());
                _logger.LogInformation("Sending request with Correlation ID: {0}", request.Headers.GetValues("X-Correlation-ID").FirstOrDefault());
            }

            var startTime = DateTime.UtcNow;
            _logger.LogInformation("Initiating Api request for the url: '{request.RequestUri}' with method: '{request.Method}'", request.RequestUri, request.Method);

            if (request.Content != null)
            {
                var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogDebug("Request body: '{RequestBody}'", requestBody);
            }


            var response = await base.SendAsync(request, cancellationToken);

            var stopTime = DateTime.UtcNow;
            var duration = stopTime - startTime;

            _logger.LogInformation("Response received. Success: {IsSuccess}, Status Code: {StatusCode}, URL: {RequestUri}, Duration: {Duration}ms",
             response.IsSuccessStatusCode, response.StatusCode, request.RequestUri, duration.TotalMilliseconds);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogDebug("Response body for failed request: {ResponseBody}", responseBody);
            }
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending a request to {RequestUri} with method {Method}", request.RequestUri, request.Method);
            throw;
        }
    }
}