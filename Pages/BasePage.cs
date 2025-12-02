using OpenQA.Selenium;
namespace SharpAutomation.Pages;

public abstract class BasePage(IWebDriver driver)
{
    protected readonly IWebDriver _driver = driver ?? throw new ArgumentNullException(nameof(driver));

    protected virtual IWebElement SubmitButton => 
        _driver.FindElement(By.ClassName("submit-button"));

    public void Click(By locator) => WaitHelper.SafeClick(driver, locator);

    public void EnterText(By locator, string text)
    {
        WaitHelper.WaitForElementVisible(driver, locator).Clear();
        WaitHelper.WaitForElementVisible(driver, locator).SendKeys(text);
    }

    public string GetText(By locator)
    {
        return WaitHelper.WaitForElementVisible(driver, locator).Text.Trim();
    }
}
