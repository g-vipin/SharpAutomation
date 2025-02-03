using Allure.NUnit;
using FluentAssertions;
using OpenQA.Selenium;

namespace SharpAutomation.Tests
{
    [AllureNUnit]
    public class TitleTest
    {

        [Test]
        public void NavigateToBaseUrl_ShouldMatchExpectedTitle()
        {
            var driver = GlobalSetUp.GetService<IWebDriver>();
            var configService = GlobalSetUp.ConfigurationHelper;
            var baseUrl = configService.GetConfig("AppSettings:BaseUrl");
            driver.Navigate().GoToUrl(baseUrl);
            var actualTitle = driver.Title;
            actualTitle.Should().Be("Swag Labs");
        }

    }
}
