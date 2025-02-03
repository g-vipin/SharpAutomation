using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using BoDi;
using OpenQA.Selenium;
using TechTalk.SpecFlow;

namespace SharpAutomation
{
    [Binding]
    public class Hooks(IObjectContainer objectContainer, ScenarioContext scenarioContext)
    {
        private readonly IObjectContainer _objectContainer = objectContainer;
        private readonly ScenarioContext _scenarioContext = scenarioContext;

        private IWebDriver? _driver;
        private static ExtentReports? _extentReports;
        private static ExtentSparkReporter? _extentSparkReporter;
        private ExtentTest? _extentTest;

        [BeforeTestRun]
        public static void InitializeReport()
        {
            var reportPath = Path.Combine(TestContext.CurrentContext.WorkDirectory,"TestResults", "ExtentTestReport.html");
            _extentSparkReporter = new ExtentSparkReporter(reportPath);
            _extentReports = new ExtentReports();
            _extentReports.AttachReporter(_extentSparkReporter);

        }

        [BeforeScenario(Order = -1)]
        public void InitializeWebDriver()
        {
            _driver = GlobalSetUp.GetService<IWebDriver>();
            _objectContainer.RegisterInstanceAs(_driver);

            if (_driver == null)
            {
                throw new InvalidOperationException("WebDriver could not be initialized.");
            }
        }

        [BeforeScenario(Order = 0)]
        public void LaunchBaseUrl()
        {
            try
            {
                var configService = GlobalSetUp.ConfigurationHelper;
                var baseUrl = configService.GetConfig("AppSettings:BaseUrl");

                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    throw new InvalidOperationException("Base URL is not configured properly.");
                }

                if (_driver != null)
                {
                    _driver.Navigate().GoToUrl(baseUrl);

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error launching Base URL: {ex.Message}");
                throw;
            }
        }

        [BeforeScenario(Order = 1)]
        public void CreateTestNodeForReport()
        {
            if (_extentReports != null)
            {
                _extentTest = _extentReports.CreateTest(_scenarioContext.ScenarioInfo.Title);
                _scenarioContext["ExtentTest"] = _extentTest;
            }
        }

        [AfterStep]
        public void LogStepResults()
        {
            var stepType = _scenarioContext.StepContext.StepInfo.StepDefinitionType.ToString();
            var stepInfo = _scenarioContext.StepContext.StepInfo.Text;

            if (_extentTest != null)
            {

                if (_scenarioContext.TestError == null)
                {
                    _extentTest.Log(AventStack.ExtentReports.Status.Pass, $"{stepType}: {stepInfo}");
                }
                else
                {
                    _extentTest.Log(AventStack.ExtentReports.Status.Fail, $"{stepType}: {stepInfo}");
                    _extentTest.Log(AventStack.ExtentReports.Status.Fail, _scenarioContext.TestError.Message);

                    AddScreenshotToReport();
                }
            }
        }

        private void AddScreenshotToReport()
        {
            try
            {
                if (_driver is ITakesScreenshot takesScreenshot)
                {
                    var screenshot = takesScreenshot.GetScreenshot();
                    var scenarioTitle = _scenarioContext.ScenarioInfo.Title;
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var screenshotDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestResults", "ScreenShots");
                    Directory.CreateDirectory(screenshotDirectory);
                    var screenshotPath = Path.Combine(screenshotDirectory, $"{scenarioTitle}_{timestamp}.png");

                    screenshot.SaveAsFile(screenshotPath);
                    _extentTest?.AddScreenCaptureFromPath(screenshotPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing screenshot: {ex.Message}");
            }
        }

        [AfterScenario(Order = 0)]
        public void DisposeWebDriver()
        {
            try
            {
                _driver?.Quit();
                _driver?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while disposing WebDriver: {ex.Message}");
            }
        }

        [AfterScenario(Order = int.MaxValue)]
        public void LogScenarioEnd()
        {
            _extentTest?.Log(AventStack.ExtentReports.Status.Info, "Scenario Ended");
        }

        [AfterTestRun]
        public static void TearDownReport()
        {
            _extentReports?.Flush();
        }
    }
}
