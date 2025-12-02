using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V130.Network;
using OpenQA.Selenium.Firefox;
using SharpAutomation.Config;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using Log = Serilog.Log;

namespace SharpAutomation.Helpers
{
    public sealed class DriverFactory
    {
        private static readonly object _lock = new();

        public IWebDriver CreateWebDriver(string browser)
        {
            lock (_lock)
            {
                return browser.ToLower() switch
                {
                    "chrome" => CreateChromeDriver(),
                    "firefox" => CreateFirefoxDriver(),
                    _ => throw new NotSupportedException($"Browser '{browser}' is not supported.")
                };
            }
        }

        private IWebDriver CreateChromeDriver()
        {
            var options = new ChromeOptions();
            options.AddArguments("--disable-notifications", "--no-sandbox");

            var driver = new ChromeDriver(options);

            var devTools = ((IDevTools)driver).GetDevToolsSession(new DevToolsOptions
            {
                ProtocolVersion = 132
            });

            devTools.SendCommand(new EnableCommandSettings());

            var correlationId = CorrelationContextAccessor.Current?.CorrelationId ?? Guid.NewGuid().ToString();
            var headers = new Headers();
            headers.Add("X-Correlation-ID", correlationId);

            devTools.SendCommand(new SetExtraHTTPHeadersCommandSettings
            {
                Headers = headers
            });

            Log.Information("[CDP] Injected X-Correlation-ID: {CorrelationId}", correlationId);
            return driver;
        }

        private IWebDriver CreateFirefoxDriver()
        {
            new DriverManager().SetUpDriver(new FirefoxConfig());
            var options = new FirefoxOptions();
            options.SetLoggingPreference(LogType.Browser, LogLevel.All);
            return new FirefoxDriver(options);
        }
    }
}
