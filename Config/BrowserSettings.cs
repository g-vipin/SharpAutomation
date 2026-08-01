namespace SharpAutomation.Config;

public class BrowserSettings
{
    public string Browser { get; set; } = "chrome";
    public string BaseUrl { get; set; } = string.Empty;
    public int ImplicitWait { get; set; }
    public bool Headless { get; set; }
}
