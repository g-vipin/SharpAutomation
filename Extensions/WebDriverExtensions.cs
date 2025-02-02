using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace SharpAutomation.Extensions;
public static class WebDriverExtensions
{
    public static IWebElement FindWebElement(this IWebDriver driver, By locator)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(10));
        wait.Until(ExpectedConditions.ElementExists(locator));  
        return driver.FindElement(locator);
    }
}