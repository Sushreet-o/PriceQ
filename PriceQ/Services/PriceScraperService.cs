using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;

namespace PriceQ.Services
{
    public class PriceScraperService
    {
        // Make sure this name is EXACTLY GetDarazPrice
        public string GetDarazPrice(string url)
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless"); // Runs browser in background
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");

            using (IWebDriver driver = new ChromeDriver(options))
            {
                try
                {
                    driver.Navigate().GoToUrl(url);

                    // Wait for the price element to appear
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                    // This class is what Daraz uses for the main price
                    var priceElement = wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("pdp-price_type_normal")));

                    return priceElement.Text;
                }
                catch (Exception)
                {
                    return "Price Unavailable";
                }
            }
        }
    }
}