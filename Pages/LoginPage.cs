using System.Diagnostics;
using OpenQA.Selenium;
using SharpAutomation.Extensions;

namespace SharpAutomation.Pages;
public class LoginPage : Page
{
    public LoginPage(IWebDriver driver) : base(driver)
    {
    }

    private IWebElement UserNameField => _driver.FindWebElement(By.Id("user-name"));
    private IWebElement PasswordField => _driver.FindWebElement(By.Id("password"));

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