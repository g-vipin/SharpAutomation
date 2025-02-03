using Allure.NUnit;
using FluentAssertions;
using OpenQA.Selenium;

namespace SharpAutomation.Tests
{   [AllureNUnit]
    public class UrlTest
    {
        [Test]
        public void NavigateToBaseUrl_ShouldMatchExpectedUrl()
        {
            var driver = GlobalSetUp.GetService<IWebDriver>();
            var configService = GlobalSetUp.ConfigurationHelper;
            var baseUrl = configService.GetConfig("AppSettings:BaseUrl");
            driver.Navigate().GoToUrl(baseUrl);
            var actualUrl = driver.Url;
            actualUrl.Should().Be(baseUrl, "The navigated URL should match the base URL.");
        }

    }
}
