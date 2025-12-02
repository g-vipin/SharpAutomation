using System.Diagnostics;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using BoDi;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using Serilog;
using SharpAutomation.Helpers;
using TechTalk.SpecFlow;

namespace SharpAutomation
{
    [Binding]
    public class Hooks
    {
        private readonly IObjectContainer _objectContainer;
        private IServiceScope? _scope;
        private IWebDriver? _driver;
        private readonly ScenarioContext _scenarioContext;
        private readonly ILogger _logger = Log.ForContext<Hooks>();

        private static ExtentReports? _extentReports;
        private static ExtentSparkReporter? _extentReporter;
        private ExtentTest? _extentTest;

        public Hooks(IObjectContainer objectContainer, ScenarioContext scenarioContext)
        {
            _objectContainer = objectContainer;
            _scenarioContext = scenarioContext;
        }

        [BeforeTestRun(Order = 0)]
        public static void InitializeReporting()
        {
            var resultDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestResults");
            Directory.CreateDirectory(resultDir);

            var reportPath = Path.Combine(resultDir, "ExtentReport.html");
            var traceLogPath = Path.Combine(resultDir, "trace.log");

            Trace.Listeners.Add(new TextWriterTraceListener(traceLogPath));
            Trace.Listeners.Add(new ConsoleTraceListener());
            Trace.AutoFlush = true;

            _extentReporter = new ExtentSparkReporter(reportPath);
            _extentReports = new ExtentReports();
            _extentReports.AttachReporter(_extentReporter);

            Trace.TraceInformation("Extent Report & Trace initialized at: " + resultDir);
        }

        [BeforeScenario(Order = -1)]
        public void InitializeScenario()
        {
            _scope = GlobalSetUp.CreateTestScope();

            var correlationContext = _scope.ServiceProvider.GetRequiredService<CorrelationContext>();
            CorrelationContextAccessor.Current = correlationContext;

            _driver = _scope.ServiceProvider.GetRequiredService<IWebDriver>();
            _objectContainer.RegisterInstanceAs(_driver);
            _objectContainer.RegisterInstanceAs(correlationContext);

            var scenarioName = _scenarioContext.ScenarioInfo.Title;
            _extentTest = _extentReports?.CreateTest(scenarioName)
                .AssignCategory(_scenarioContext.ScenarioInfo.Tags);

            Trace.TraceInformation($"Starting Scenario: {scenarioName}");
            Trace.TraceInformation($"[Correlation ID]: {correlationContext.CorrelationId}");
        }

        public void BeforeStep()
        {
            Trace.TraceInformation($"Executing Step: {_scenarioContext.StepContext.StepInfo.Text}");
        }

        [AfterStep]
        public void LogStepResults()
        {
            var stepType = _scenarioContext.StepContext.StepInfo.StepDefinitionType.ToString();
            var stepInfo = _scenarioContext.StepContext.StepInfo.Text;

            if (_scenarioContext.TestError == null)
            {
                _extentTest?.Log(AventStack.ExtentReports.Status.Pass, $"{stepType}: {stepInfo}");
                Trace.TraceInformation($"{stepType}: {stepInfo} - ✅ Passed");
            }
            else
            {
                _extentTest?.Log(AventStack.ExtentReports.Status.Fail, $"{stepType}: {stepInfo}");
                _extentTest?.Log(AventStack.ExtentReports.Status.Fail, _scenarioContext.TestError.Message);

                AddScreenshotToReport();
                Trace.TraceError($"{stepType}: {stepInfo} - ❌ Failed");
                Trace.TraceError($"Error: {_scenarioContext.TestError.Message}");
            }
        }

        [AfterScenario(Order = 100)]
        public void TearDownScenario()
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
            finally
            {
                _scope?.Dispose();
                Trace.TraceInformation($"Scenario Completed: {_scenarioContext.ScenarioInfo.Title}");
            }
        }

        [AfterTestRun(Order = 100)]
        public static void FinalizeReport()
        {
            try
            {
                _extentReports?.Flush();
                Trace.TraceInformation("Extent Report flushed successfully.");
                Trace.Flush();
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error during report finalization: {ex.Message}");
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

                    var screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "TestResults", "Screenshots");
                    Directory.CreateDirectory(screenshotDir);

                    var screenshotPath = Path.Combine(screenshotDir, $"{scenarioTitle}_{timestamp}.png");
                    screenshot.SaveAsFile(screenshotPath);

                    _extentTest?.AddScreenCaptureFromPath(screenshotPath);
                    Trace.TraceInformation($"Screenshot saved: {screenshotPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error capturing screenshot.");
                Trace.TraceError($"Error capturing screenshot: {ex.Message}");
            }
        }
    }
}
