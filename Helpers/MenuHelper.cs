using OpenQA.Selenium;
public class MenuHelper
{
    private readonly IWebDriver _driver;

    public MenuHelper(IWebDriver driver)
    {
        _driver = driver;
    }

    private By MenuItems => By.CssSelector("ul.menu li");

    public IList<IWebElement> GetItems() =>
        _driver.FindElements(MenuItems);

    public bool HasItem(string text) =>
        GetItems().Any(x => x.Text.Trim().Equals(text, StringComparison.OrdinalIgnoreCase));

    public void ClickItem(string text)
    {
        var item = GetItems()
            .FirstOrDefault(x => x.Text.Trim().Equals(text, StringComparison.OrdinalIgnoreCase));

        if (item == null)
            throw new Exception($"Menu item {text} not found");

        item.Click();
    }
}
