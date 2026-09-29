using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using PriceQ.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PriceQ.Services
{
    public class FlipkartService : IStoreSearchService
    {
        public string StoreName => "Flipkart";

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
                        "https://www.flipkart.com/search?q="
                        + Uri.EscapeDataString(searchQuery);

                    driver.Navigate().GoToUrl(searchUrl);

                    var wait = new WebDriverWait(
                        driver,
                        TimeSpan.FromSeconds(15)
                    );

                    await Task.Run(() =>
                    {
                        wait.Until(d =>
                            d.FindElements(
                                By.CssSelector("div[data-id]")
                            ).Count > 0
                        );
                    });

                    var products = driver.FindElements(
                        By.CssSelector("div[data-id]")
                    );

                    foreach (var product in products)
                    {
                        try
                        {
                            string name = "";
                            string price = "";
                            string productUrl = "";
                            string imageUrl = "";

                            // Product name
                            try
                            {
                                var nameElement = product.FindElement(
                                    By.CssSelector(
                                        "a[href*='/p/']"
                                    )
                                );

                                name = nameElement.Text.Trim();

                                productUrl =
                                    nameElement.GetAttribute("href")
                                    ?? "";
                            }
                            catch
                            {
                                // Try alternative product title selector
                                try
                                {
                                    var titleElement =
                                        product.FindElement(
                                            By.CssSelector(
                                                "div.KzDlHZ"
                                            )
                                        );

                                    name = titleElement.Text.Trim();
                                }
                                catch
                                {
                                    continue;
                                }
                            }

                            // Product URL
                            if (string.IsNullOrWhiteSpace(productUrl))
                            {
                                try
                                {
                                    var linkElement =
                                        product.FindElement(
                                            By.CssSelector(
                                                "a[href*='/p/']"
                                            )
                                        );

                                    productUrl =
                                        linkElement.GetAttribute("href")
                                        ?? "";
                                }
                                catch
                                {
                                    continue;
                                }
                            }

                            // Price
                            try
                            {
                                var priceElement =
                                    product.FindElement(
                                        By.CssSelector(
                                            "div.Nx9bqj"
                                        )
                                    );

                                price = priceElement.Text.Trim();
                            }
                            catch
                            {
                                price = "Price unavailable";
                            }

                            // Product image
                            try
                            {
                                var imageElement =
                                    product.FindElement(
                                        By.CssSelector("img")
                                    );

                                imageUrl =
                                    imageElement.GetAttribute("src")
                                    ?? "";
                            }
                            catch
                            {
                                imageUrl = "";
                            }

                            if (string.IsNullOrWhiteSpace(name))
                                continue;

                            if (string.IsNullOrWhiteSpace(productUrl))
                                continue;

                            if (productUrl.StartsWith("/"))
                            {
                                productUrl =
                                    "https://www.flipkart.com"
                                    + productUrl;
                            }

                            results.Add(
                                new StoreProductResult
                                {
                                    Name = name,

                                    Category = "General",

                                    Price = price,

                                    PreviousPrice = "",

                                    Discount = "",

                                    StoreName = "Flipkart",

                                    StoreLogoUrl =
                                        "/images/logos/Flipkart_logo.svg.png",

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
                            // Skip products that cannot be read
                        }
                    }
                }
                catch
                {
                    // Flipkart could not be accessed.
                    // Return an empty list instead of crashing PriceQ.
                }
            }

            return results;
        }
    }
}