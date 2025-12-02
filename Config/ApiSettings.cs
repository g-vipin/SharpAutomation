namespace SharpAutomation.Config;

public class ApiSettings
{
    public required string BaseUrl { get; set; }
    public int TimeoutSeconds { get; set; } = 30;

    public string ContentType { get; set; } = "application/json";
}
