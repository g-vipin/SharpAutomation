using OpenQA.Selenium;
using SharpAutomation.Extensions;
namespace SharpAutomation.Pages;

public abstract class Page(IWebDriver driver)
{
    protected readonly IWebDriver _driver = driver ?? throw new ArgumentNullException(nameof(driver));

    protected virtual IWebElement SubmitButton => 
        _driver.FindWebElement(By.ClassName("submit-button"));
}
