using Allure.NUnit;
using FluentAssertions;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using SharpAutomation.Config;

namespace SharpAutomation.Tests
{
    [AllureNUnit]
    public class TitleTest
    {

        [Test]
        public void NavigateToBaseUrl_ShouldMatchExpectedTitle()
        {
            var driver = GlobalSetUp.GetService<IWebDriver>();
            var config = GlobalSetUp.GetService<IOptions<BrowserSettings>>().Value;
            driver.Navigate().GoToUrl(config.Browser);
            var actualTitle = driver.Title;
            actualTitle.Should().Be("Swag Labs");
        }

    }
}
