using OpenQA.Selenium;
namespace SharpAutomation.Pages;

public abstract class Page
{
    protected readonly IWebDriver _driver;

    protected Page(IWebDriver driver)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
    }

   protected virtual IWebElement SubmitButton => 
        _driver.FindElement(By.ClassName("submit-button"));
}
