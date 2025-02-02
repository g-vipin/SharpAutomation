using OpenQA.Selenium;
using SharpAutomation.Pages;
using TechTalk.SpecFlow;

namespace SharpAutomation.StepDefenitions
{
    [Binding]
    public sealed class LoginSteps
    {
        private readonly IWebDriver _driver;

        private readonly LoginPage _loginPage;

        public LoginSteps(IWebDriver driver)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _loginPage = new LoginPage(_driver);
        }

        [Given(@"^I enter valid credentials and Perform Login$")]
        public void GivenIEnterValidCredentialsAndLogin()
        {
            _loginPage.Login();

        }

        [Then(@"^I should be redirected to the dashboard$")]
        public void ThenIShouldBeRedirectedToTheDashboard()
        {
            var currentUrl = _driver.Url;
            Assert.That(currentUrl, Is.EqualTo("https://www.saucedemo.com/inventory.html"));
        }
    }
}
