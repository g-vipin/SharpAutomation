using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using Serilog;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace SharpAutomation.Helpers;
public sealed class DriverFactory
{
    private static readonly Object _lock = new object();
    public IWebDriver CreateWebDriver(string browserConfig)
    {
        lock (_lock)
        {
            Log.Logger.Information("Creating WebDriver for browser: {BrowserConfig}", browserConfig);

            if (string.IsNullOrWhiteSpace(browserConfig))
            {
                throw new ArgumentException("Browser configuration cannot be null or empty.");
            }

            return browserConfig.ToLower() switch
            {
                "chrome" => CreateChromeDriver(),
                "firefox" => CreateFirefoxDriver(),
                _ => throw new NotSupportedException($"Browser '{browserConfig}' is not supported."),
            };
        }
    }

    private IWebDriver CreateChromeDriver()
    {
        new DriverManager().SetUpDriver(new ChromeConfig());
        var options = new ChromeOptions();
        return new ChromeDriver(options);
    }

    private IWebDriver CreateFirefoxDriver()
    {
        new DriverManager().SetUpDriver(new FirefoxConfig());
        var options = new FirefoxOptions();
        return new FirefoxDriver(options);
    }
}
