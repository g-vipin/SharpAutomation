namespace SharpAutomation.API;

public sealed class MicroserviceApiClientFactory : IMicroserviceApiClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApiResiliencePolicyFactory _policyFactory;

    public MicroserviceApiClientFactory(IHttpClientFactory httpClientFactory, ApiResiliencePolicyFactory policyFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _policyFactory = policyFactory ?? throw new ArgumentNullException(nameof(policyFactory));
    }

    public IHttpApiClient CreateClient(string clientName)
    {
        if (string.IsNullOrWhiteSpace(clientName))
        {
            throw new ArgumentException("Microservice client name cannot be empty.", nameof(clientName));
        }

        return new HttpApiClient(_httpClientFactory.CreateClient(clientName), _policyFactory);
    }
}
