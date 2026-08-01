using System.Net.Http.Headers;
using System.Text;
using SharpAutomation.Config;

namespace SharpAutomation.API;

public sealed class ApiAuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly ApiAuthSettings _authSettings;

    public ApiAuthenticationDelegatingHandler(ApiAuthSettings authSettings)
    {
        _authSettings = authSettings ?? throw new ArgumentNullException(nameof(authSettings));
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ApplyAuthentication(request);
        return base.SendAsync(request, cancellationToken);
    }

    private void ApplyAuthentication(HttpRequestMessage request)
    {
        if (request.Headers.Authorization is not null)
        {
            return;
        }

        switch (_authSettings.Scheme.Trim().ToLowerInvariant())
        {
            case "":
            case "none":
                return;

            case "basic":
                var credentials = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{_authSettings.Username}:{_authSettings.Password}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                return;

            case "bearer":
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authSettings.Token);
                return;

            case "apikey":
            case "api-key":
                if (string.IsNullOrWhiteSpace(_authSettings.HeaderName))
                {
                    throw new InvalidOperationException("Api key authentication requires HeaderName.");
                }

                request.Headers.Remove(_authSettings.HeaderName);
                request.Headers.Add(_authSettings.HeaderName, _authSettings.ApiKey);
                return;

            default:
                throw new NotSupportedException($"Authentication scheme '{_authSettings.Scheme}' is not supported.");
        }
    }
}
