using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using PriceQ.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PriceQ.Services
{
    public class HamroBazarService : IStoreSearchService
    {
        public string StoreName => "HamroBazar";

        public async Task<List<StoreProductResult>> SearchAsync(string searchQuery)
        {
            var results = new List<StoreProductResult>();

            var options = new ChromeOptions();

            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--window-size=1920,1080");

            using (IWebDriver driver = new ChromeDriver(options))
            {
                try
                {
                    string searchUrl =
                        "https://hamrobazaar.com/search/product?q="
                        + Uri.EscapeDataString(searchQuery);

                    driver.Navigate().GoToUrl(searchUrl);

                    var wait = new WebDriverWait(
                        driver,
                        TimeSpan.FromSeconds(15)
                    );

                    await Task.Run(() =>
                    {
                        wait.Until(d =>
                            d.FindElements(By.CssSelector("body")).Count > 0
                        );
                    });

                    var products = driver.FindElements(
                        By.CssSelector("a")
                    );

                    foreach (var product in products)
                    {
                        try
                        {
                            string productUrl =
                                product.GetAttribute("href");

                            string name =
                                product.Text?.Trim() ?? "";

                            if (string.IsNullOrWhiteSpace(name))
                                continue;

                            if (string.IsNullOrWhiteSpace(productUrl))
                                continue;

                            if (!productUrl.Contains("hamrobazaar"))
                                continue;

                            IWebElement imageElement = null;

                            try
                            {
                                imageElement =
                                    product.FindElement(
                                        By.CssSelector("img")
                                    );
                            }
                            catch
                            {
                                // Product may not have an image
                            }

                            string imageUrl = "";

                            if (imageElement != null)
                            {
                                imageUrl =
                                    imageElement.GetAttribute("src")
                                    ?? "";
                            }

                            results.Add(
                                new StoreProductResult
                                {
                                    Name = name,

                                    Category = "General",

                                    Price = "",

                                    PreviousPrice = "",

                                    Discount = "",

                                    StoreName = "HamroBazar",

                                    StoreLogoUrl =
                                        "/images/logos/hamrobazar_logo.svg.png",

                                    ImageUrl = imageUrl,

                                    ProductUrl = productUrl,

                                    Availability = "Available",

                                    SellerName = ""
                                }
                            );

                            if (results.Count >= 20)
                                break;
                        }
                        catch
                        {
                            // Skip invalid product
                        }
                    }
                }
                catch
                {
                    // Return empty list if HamroBazar
                    // cannot be accessed.
                }
            }

            return results;
        }
    }
}