using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

public static class WaitHelper
{
    public static IWebElement WaitForElementVisible(IWebDriver driver, By locator, int seconds = 15)
    {
        return new WebDriverWait(driver, TimeSpan.FromSeconds(seconds))
            .Until(ExpectedConditions.ElementIsVisible(locator));
    }

    public static IWebElement WaitForElementClickable(IWebDriver driver, By locator, int seconds = 15)
    {
        return new WebDriverWait(driver, TimeSpan.FromSeconds(seconds))
            .Until(ExpectedConditions.ElementToBeClickable(locator));
    }

    public static bool WaitForElementNotVisible(IWebDriver driver, By locator, int seconds = 15)
    {
        return new WebDriverWait(driver, TimeSpan.FromSeconds(seconds))
            .Until(ExpectedConditions.InvisibilityOfElementLocated(locator));
    }

    public static void WaitForAlert(IWebDriver driver, int seconds = 10)
    {
        new WebDriverWait(driver, TimeSpan.FromSeconds(seconds))
            .Until(ExpectedConditions.AlertIsPresent());
    }

    public static void WaitForPageLoad(IWebDriver driver, int seconds = 15)
    {
        new WebDriverWait(driver, TimeSpan.FromSeconds(seconds)).Until(d =>
        {
            return ((IJavaScriptExecutor)d)
                .ExecuteScript("return document.readyState")
                .Equals("complete");
        });
    }

    public static void WaitForSPAContent(IWebDriver driver, By locator, int seconds = 20)
    {
        new WebDriverWait(driver, TimeSpan.FromSeconds(seconds))
            .Until(ExpectedConditions.ElementExists(locator));
    }

    public static void SafeClick(IWebDriver driver, By locator, int retries = 3)
    {
        int attempt = 0;

        while (attempt < retries)
        {
            try
            {
                WaitForElementClickable(driver, locator).Click();
                return;
            }
            catch (StaleElementReferenceException)
            {
                attempt++;
            }
        }

        throw new Exception($"Unable to click element after {retries} retries: {locator}");
    }

    public static void WaitForTabCount(IWebDriver driver, int expectedCount, int seconds = 20)
    {
        new WebDriverWait(driver, TimeSpan.FromSeconds(seconds))
            .Until(d => d.WindowHandles.Count == expectedCount);
    }
}
