using OpenQA.Selenium;
public class BreadcrumbHelper
{
    private readonly IWebDriver _driver;

    public BreadcrumbHelper(IWebDriver driver)
    {
        _driver = driver;
    }

    private By Crumbs => By.CssSelector("nav.breadcrumb *");

    public IList<IWebElement> GetBreadcrumbs() =>
        _driver.FindElements(Crumbs);

    public string GetFullBreadcrumbText()
    {
        return string.Join(" > ",
            GetBreadcrumbs().Select(x => x.Text.Trim()));
    }

    public void ClickBreadcrumb(string label)
    {
        var crumb = GetBreadcrumbs()
            .FirstOrDefault(c => c.Text.Trim().Equals(label, StringComparison.OrdinalIgnoreCase));

        if (crumb == null)
            throw new Exception($"Breadcrumb '{label}' not found");

        crumb.Click();
    }
}
