namespace SharpAutomation.Config;

public class BrowserSettings
{
    public string Browser { get; set; } = "chrome";
    public int ImplicitWait { get; set; }
    public bool Headless { get; set; }
}