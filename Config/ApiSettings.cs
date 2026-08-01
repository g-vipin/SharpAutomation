namespace SharpAutomation.Config;

public class ApiSettings : ApiEndpointSettings
{
    public Dictionary<string, ApiEndpointSettings> Microservices { get; set; } = [];

    public ApiResilienceSettings Resilience { get; set; } = new();
}

public class ApiEndpointSettings
{
    public required string BaseUrl { get; set; }

    public int TimeoutSeconds { get; set; } = 30;

    public int TimeoutSec { get; set; }

    public string ContentType { get; set; } = "application/json";

    public ApiAuthSettings Auth { get; set; } = new();

    public int ResolvedTimeoutSeconds => TimeoutSec > 0 ? TimeoutSec : TimeoutSeconds;
}

public class ApiAuthSettings
{
    public string Scheme { get; set; } = "None";

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Token { get; set; }

    public string? ApiKey { get; set; }

    public string? HeaderName { get; set; }
}

public class ApiResilienceSettings
{
    public int RetryCount { get; set; } = 3;

    public int RetryBaseDelayMilliseconds { get; set; } = 250;

    public int TimeoutSeconds { get; set; } = 30;

    public int CircuitBreakerFailuresBeforeBreak { get; set; } = 5;

    public int CircuitBreakerBreakSeconds { get; set; } = 30;
}
