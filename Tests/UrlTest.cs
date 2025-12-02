using Allure.NUnit;
using FluentAssertions;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using SharpAutomation.Config;

namespace SharpAutomation.Tests
{   [AllureNUnit]
    public class UrlTest
    {
        [Test]
        public void NavigateToBaseUrl_ShouldMatchExpectedUrl()
        {
            var driver = GlobalSetUp.GetService<IWebDriver>();
            var configService = GlobalSetUp.GetService<IOptions<BrowserSettings>>().Value;
            var baseUrl = configService.Browser;
            driver.Navigate().GoToUrl(baseUrl);
            var actualUrl = driver.Url;
            actualUrl.Should().Be(baseUrl, "The navigated URL should match the base URL.");
        }

    }
}
