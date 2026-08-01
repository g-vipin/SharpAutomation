namespace SharpAutomation.API;

public interface IMicroserviceApiClientFactory
{
    IHttpApiClient CreateClient(string clientName);
}
