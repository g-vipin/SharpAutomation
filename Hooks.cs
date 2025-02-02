using BoDi;
using OpenQA.Selenium;
using SharpAutomation.Helpers;
using TechTalk.SpecFlow;

namespace SharpAutomation
{
    [Binding]
    public class Hooks
    {
        private readonly IObjectContainer _objectContainer;

        private IWebDriver? _webDriver;

        public Hooks(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
        }

        [BeforeScenario(Order = -1)]
        public void InitializeWebDriver()
        {
            var configHelper = GlobalSetUp.GetService<ConfigurationHelper>();
            var driverFactory = GlobalSetUp.GetService<DriverFactory>();
            var browserConfig = configHelper.GetConfig("AppSettings:Browser");

            _webDriver = driverFactory.CreateWebDriver(browserConfig);
            _objectContainer.RegisterInstanceAs<IWebDriver>(_webDriver);

        }

        [BeforeScenario(Order = 0)]
        public void LaunchBaseUrl()
        {
            if (_webDriver == null)
            {
                throw new InvalidOperationException("WebDriver is not initialized.");
            }
            var configService = GlobalSetUp.ConfigurationHelper;
            var baseUrl = configService.GetConfig("AppSettings:BaseUrl");
            _webDriver.Navigate().GoToUrl(baseUrl);

        }

        [AfterScenario]
        public void AfterScenario()
        {
            try
            {
                _webDriver?.Quit();
                _webDriver?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while disposing WebDriver: {ex.Message}");
            }

        }
    }
}
