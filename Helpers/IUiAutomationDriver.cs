using OpenQA.Selenium;

namespace SharpAutomation.Helpers;

public interface IUiAutomationDriver : IDisposable
{
    string EngineName { get; }

    IWebDriver? SeleniumDriver { get; }
}
