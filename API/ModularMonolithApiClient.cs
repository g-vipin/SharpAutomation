namespace SharpAutomation.API;

public sealed class ModularMonolithApiClient : HttpApiClient, IModularMonolithApiClient
{
    public ModularMonolithApiClient(HttpClient client, ApiResiliencePolicyFactory policyFactory)
        : base(client, policyFactory)
    {
    }
}
