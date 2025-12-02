using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

public static class BrowserHelper
{
    public static string SwitchToNewTab(IWebDriver driver, int waitSeconds = 20)
    {
        string current = driver.CurrentWindowHandle;

        WaitHelper.WaitForTabCount(driver, driver.WindowHandles.Count + 1, waitSeconds);

        string newTab = driver.WindowHandles.First(h => h != current);
        driver.SwitchTo().Window(newTab);

        return newTab;
    }

    public static void CloseTabAndReturn(IWebDriver driver, string parentWindow, int waitSeconds = 20)
    {
        driver.Close();

        WaitHelper.WaitForTabCount(driver, 1, waitSeconds);

        driver.SwitchTo().Window(parentWindow);
    }

    public static void SwitchToWindowByTitle(IWebDriver driver, string title, int seconds = 20)
    {
        new WebDriverWait(driver, TimeSpan.FromSeconds(seconds)).Until(d =>
        {
            foreach (var handle in d.WindowHandles)
            {
                d.SwitchTo().Window(handle);
                if (d.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        });
    }

    public static void SwitchToWindowByUrl(IWebDriver driver, string urlPart, int seconds = 20)
    {
        new WebDriverWait(driver, TimeSpan.FromSeconds(seconds)).Until(d =>
        {
            foreach (var handle in d.WindowHandles)
            {
                d.SwitchTo().Window(handle);
                if (d.Url.Contains(urlPart))
                    return true;
            }
            return false;
        });
    }

    public static void OpenTab_DoWork_Close(IWebDriver driver, By closeButton)
    {
        string parent = driver.CurrentWindowHandle;

        string newTab = SwitchToNewTab(driver);

        WaitHelper.SafeClick(driver, closeButton);

        CloseTabAndReturn(driver, parent);
    }
    public static void SwitchToIframe(IWebDriver driver, By iframeLocator)
    {
        WaitHelper.WaitForElementVisible(driver, iframeLocator);
        driver.SwitchTo().Frame(driver.FindElement(iframeLocator));
    }

    public static void SwitchToDefault(IWebDriver driver)
    {
        driver.SwitchTo().DefaultContent();
    }

    public static void AcceptAlert(IWebDriver driver)
    {
        WaitHelper.WaitForAlert(driver);
        driver.SwitchTo().Alert().Accept();
    }

    public static void DismissAlert(IWebDriver driver)
    {
        WaitHelper.WaitForAlert(driver);
        driver.SwitchTo().Alert().Dismiss();
    }

    public static void CloseOptionalModal(IWebDriver driver, By modal, By closeButton)
    {
        try
        {
            if (driver.FindElements(modal).Any())
            {
                if (driver.FindElement(modal).Displayed)
                {
                    WaitHelper.SafeClick(driver, closeButton);
                }
            }
        }
        catch { }
    }

    public static void ClickAndHandleLoginRedirect(
        IWebDriver driver, By clickTarget, By loginForm, By username, 
        By password, By loginButton, By successLocator)
    {
        WaitHelper.SafeClick(driver, clickTarget);

        new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(d =>
        {
            return d.FindElements(loginForm).Any() || 
                   d.FindElements(successLocator).Any();
        });

        if (driver.FindElements(loginForm).Any())
        {
            driver.FindElement(username).SendKeys("admin");
            driver.FindElement(password).SendKeys("admin");
            driver.FindElement(loginButton).Click();
        }

        WaitHelper.WaitForElementVisible(driver, successLocator);
    }
}
