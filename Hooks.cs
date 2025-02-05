using System.Diagnostics;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using BoDi;
using OpenQA.Selenium;
using Serilog;
using TechTalk.SpecFlow;

namespace SharpAutomation
{
    [Binding]
    public class Hooks
    {
        private readonly IObjectContainer _objectContainer;
        private readonly ScenarioContext _scenarioContext;

        private IWebDriver? _driver;
        private static ExtentReports? _extentReports;
        private static ExtentSparkReporter? _extentSparkReporter;
        private ExtentTest? _extentTest;

        private readonly ILogger _logger = Log.ForContext<Hooks>();

        public Hooks(IObjectContainer objectContainer, ScenarioContext scenarioContext)
        {
            _objectContainer = objectContainer;
            _scenarioContext = scenarioContext;
        }

        [BeforeTestRun]
        public static void InitializeLoggingAndReport()
        {
            try
            {
                var traceLogFilePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestResults", "trace.log");
                Directory.CreateDirectory(Path.GetDirectoryName(traceLogFilePath)!);

                Trace.Listeners.Add(new TextWriterTraceListener(traceLogFilePath));
                Trace.Listeners.Add(new ConsoleTraceListener());
                Trace.AutoFlush = true;
                Trace.TraceInformation("Trace logging initialized.");

                var reportPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestResults", "ExtentTestReport.html");
                _extentSparkReporter = new ExtentSparkReporter(reportPath);
                _extentReports = new ExtentReports();
                _extentReports.AttachReporter(_extentSparkReporter);
                Trace.TraceInformation("Extent report initialized.");
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error during initialization: {ex.Message}");
                throw;
            }
        }

        [BeforeScenario(Order = -1)]
        public void InitializeWebDriver()
        {
            Trace.TraceInformation($"Starting Scenario: {_scenarioContext.ScenarioInfo.Title}");
            try
            {
                _driver = GlobalSetUp.GetService<IWebDriver>();
                _objectContainer.RegisterInstanceAs(_driver);

                if (_driver == null)
                {
                    throw new InvalidOperationException("WebDriver could not be initialized.");
                }

                _logger.Information("WebDriver initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "WebDriver initialization failed.");
                Trace.TraceError($"WebDriver initialization failed: {ex.Message}");
                throw;
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

                _driver?.Navigate().GoToUrl(baseUrl);
                _logger.Information("Navigated to Base URL.");
                Trace.TraceInformation("Navigated to Base URL.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error launching Base URL.");
                Trace.TraceError($"Error launching Base URL: {ex.Message}");
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
                _logger.Information("Extent test node created for scenario.");
                Trace.TraceInformation("Extent test node created for scenario.");
            }
        }

        [AfterStep(Order = 0)]
        public void LogStepResults()
        {
            var stepType = _scenarioContext.StepContext.StepInfo.StepDefinitionType.ToString();
            var stepInfo = _scenarioContext.StepContext.StepInfo.Text;

            if (_extentTest == null) return;

            if (_scenarioContext.TestError == null)
            {
                _extentTest.Log(AventStack.ExtentReports.Status.Pass, $"{stepType}: {stepInfo}");
                Trace.TraceInformation($"{stepType}: {stepInfo} - Passed");
            }
            else
            {
                _extentTest.Log(AventStack.ExtentReports.Status.Fail, $"{stepType}: {stepInfo}");
                _extentTest.Log(AventStack.ExtentReports.Status.Fail, _scenarioContext.TestError.Message);
                AddScreenshotToReport();
                Trace.TraceError($"{stepType}: {stepInfo} - Failed");
                Trace.TraceError($"Error: {_scenarioContext.TestError.Message}");
            }
        }

        [AfterScenario(Order = 0)]
        public void DisposeWebDriver()
        {
            try
            {
                _driver?.Quit();
                _driver?.Dispose();
                _logger.Information("WebDriver disposed successfully.");
                Trace.TraceInformation("WebDriver disposed successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while disposing WebDriver.");
                Trace.TraceError($"Error while disposing WebDriver: {ex.Message}");
            }
        }

        [AfterScenario(Order = int.MaxValue)]
        public void LogScenarioEnd()
        {
            Trace.TraceInformation($"Completed Scenario: {_scenarioContext.ScenarioInfo.Title}");
            _extentTest?.Log(AventStack.ExtentReports.Status.Info, "Scenario Ended");
        }

        [AfterTestRun]
        public static void TearDownLoggingAndReport()
        {
            try
            {
                _extentReports?.Flush();
                Trace.TraceInformation("Extent report flushed.");
                Trace.TraceInformation("Trace logging finalized.");
                Trace.Flush();
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error during teardown: {ex.Message}");
            }
        }

        private void AddScreenshotToReport()
        {
            try
            {
                if (_driver is ITakesScreenshot takesScreenshot)
                {
                    var screenshot = takesScreenshot.GetScreenshot();
                    var scenarioTitle = _scenarioContext.ScenarioInfo.Title.Replace(" ", "_");
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var screenshotDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestResults", "Screenshots");
                    Directory.CreateDirectory(screenshotDirectory);
                    var screenshotPath = Path.Combine(screenshotDirectory, $"{scenarioTitle}_{timestamp}.png");

                    screenshot.SaveAsFile(screenshotPath);
                    _extentTest?.AddScreenCaptureFromPath(screenshotPath);
                    Trace.TraceInformation($"Screenshot saved at: {screenshotPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error capturing screenshot: {ex.Message}");
                Trace.TraceError($"Error capturing screenshot: {ex.Message}");
            }
        }
    }
}
