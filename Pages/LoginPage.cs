using System.Diagnostics;
using OpenQA.Selenium;
namespace SharpAutomation.Pages;
public class LoginPage : BasePage
{
    public LoginPage(IWebDriver driver) : base(driver)
    {
    }

    private IWebElement UserNameField => _driver.FindElement(By.Id("user-name"));
    private IWebElement PasswordField => _driver.FindElement(By.Id("password"));

    private string Username => "standard_user";
    private string Password => "secret_sauce";

    public void Login()
    {
        if (UserNameField.Enabled && UserNameField.Displayed)
        {
            UserNameField.Clear();
            UserNameField.SendKeys(Username);
        }

        if (PasswordField.Enabled && PasswordField.Displayed)
        {
            PasswordField.Clear();
            PasswordField.SendKeys(Password);
        }

        SubmitButton.Click();
        Trace.TraceInformation("Login Successful");

    }

}