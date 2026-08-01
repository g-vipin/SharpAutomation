using OpenQA.Selenium;

namespace SharpAutomation.Helpers;

public sealed class SeleniumUiAutomationDriver : IUiAutomationDriver
{
    public SeleniumUiAutomationDriver(IWebDriver seleniumDriver)
    {
        SeleniumDriver = seleniumDriver ?? throw new ArgumentNullException(nameof(seleniumDriver));
    }

    public string EngineName => "Selenium";

    public IWebDriver SeleniumDriver { get; }

    public void Dispose()
    {
        SeleniumDriver.Quit();
        SeleniumDriver.Dispose();
    }
}
