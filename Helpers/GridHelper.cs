using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace SharpAutomation.Helpers;

public class GridHelper
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public Dictionary<string, By> GridActions { get; }

    private readonly By _rowSelector;
    private readonly By _rowCheckbox;

    public GridHelper(IWebDriver driver, int timeoutSec = 10,
        By? rowSelector = null,
        By? rowCheckbox = null)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSec));

        GridActions = new Dictionary<string, By>();

        _rowSelector = rowSelector ?? By.XPath("//table//tr[./td]");
        _rowCheckbox = rowCheckbox ?? By.XPath(".//input[@type='checkbox']");
    }

    private IWebElement WaitFor(By locator) =>
        _wait.Until(ExpectedConditions.ElementToBeClickable(locator));

    public GridHelper Add(string action, By locator)
    {
        GridActions[action] = locator;
        return this;
    }

    private Action<By> Perform => (by) => _driver.FindElement(by).Click();

    public void Execute(string action)
    {
        if (!GridActions.TryGetValue(action, out var locator))
            throw new KeyNotFoundException($"Grid action '{action}' not found.");
        Perform(locator);
    }

    public void SelectRow(int rowIndex)
    {
        var rows = _driver.FindElements(_rowSelector);
        if (rowIndex < 1 || rowIndex > rows.Count)
            throw new ArgumentOutOfRangeException(nameof(rowIndex));

        var checkbox = rows[rowIndex - 1].FindElement(_rowCheckbox);
        if (!checkbox.Selected)
            checkbox.Click();
    }

    public void SelectRowByText(string rowText)
    {
        var rowLocator = By.XPath(
            $"//table//tr[./td[contains(normalize-space(), {EscapeForXPath(rowText)})]]"
        );

        var row = WaitFor(rowLocator);
        var checkbox = row.FindElement(_rowCheckbox);

        if (!checkbox.Selected)
            checkbox.Click();
    }

    private string EscapeForXPath(string input)
    {
        if (!input.Contains("'"))
            return $"'{input}'";

        return $"concat('{string.Join("',\"'\",'", input.Split('\''))}')";
    }
}
