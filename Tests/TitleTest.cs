using Allure.NUnit;
using FluentAssertions;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SharpAutomation.Config;

namespace SharpAutomation.Tests
{
    [AllureNUnit]
    public class TitleTest
    {

        [Test]
        public async Task NavigateToBaseUrl_ShouldMatchExpectedTitle()
        {
            var driver = GlobalSetUp.GetService<IWebDriver>();
            var config = GlobalSetUp.GetService<IOptions<BrowserSettings>>().Value;

            var actualTitle = await RetryPolicy.RetryAsync(
                action: _ =>
                {
                    driver.Navigate().GoToUrl(config.Browser);
                    return Task.FromResult(driver.Title);
                },
                condition: title => new WebDriverWait(driver, TimeSpan.FromSeconds(30))
                .Until(d => d.Title == "Swag Labs"),
                TimeSpan.FromSeconds(60),
                TimeSpan.FromMilliseconds(5000),
                retryCount: 3,
                exceptionFilter: ex => ex is WebDriverException
                || ex is WebDriverTimeoutException

            );

            actualTitle.Should().Be("Swag Labs");

        }
    }
}
