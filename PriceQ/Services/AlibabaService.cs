using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using PriceQ.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PriceQ.Services
{
    public class AlibabaService : IStoreSearchService
    {
        public string StoreName => "Alibaba";

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
                    // Alibaba search URL
                    string searchUrl =
                        "https://www.alibaba.com/trade/search?SearchText="
                        + Uri.EscapeDataString(searchQuery);

                    driver.Navigate().GoToUrl(searchUrl);

                    var wait = new WebDriverWait(
                        driver,
                        TimeSpan.FromSeconds(15)
                    );

                    // Wait for products to appear
                    wait.Until(driver =>
                        driver.FindElements(
                            By.CssSelector("div.fy23-search-card")
                        ).Count > 0
                    );

                    var products = driver.FindElements(
                        By.CssSelector("div.fy23-search-card")
                    );

                    foreach (var product in products)
                    {
                        try
                        {
                            string name = string.Empty;
                            string price = string.Empty;
                            string productUrl = string.Empty;
                            string imageUrl = string.Empty;

                            // Product name
                            var nameElements = product.FindElements(
                                By.CssSelector(
                                    "h2.search-card-e-title"
                                )
                            );

                            if (nameElements.Count > 0)
                            {
                                name = nameElements[0].Text.Trim();
                            }

                            // Price
                            var priceElements = product.FindElements(
                                By.CssSelector(
                                    "div.search-card-e-price-main"
                                )
                            );

                            if (priceElements.Count > 0)
                            {
                                price = priceElements[0].Text.Trim();
                            }

                            // Product link
                            var linkElements = product.FindElements(
                                By.CssSelector("a")
                            );

                            if (linkElements.Count > 0)
                            {
                                productUrl =
                                    linkElements[0]
                                        .GetAttribute("href");
                            }

                            // Product image
                            var imageElements = product.FindElements(
                                By.CssSelector("img")
                            );

                            if (imageElements.Count > 0)
                            {
                                imageUrl =
                                    imageElements[0]
                                        .GetAttribute("src");

                                // Some websites use lazy loading
                                if (string.IsNullOrEmpty(imageUrl))
                                {
                                    imageUrl =
                                        imageElements[0]
                                            .GetAttribute("data-src");
                                }
                            }

                            // Fix relative URL
                            if (!string.IsNullOrEmpty(productUrl) &&
                                productUrl.StartsWith("//"))
                            {
                                productUrl =
                                    "https:" + productUrl;
                            }
                            else if (!string.IsNullOrEmpty(productUrl) &&
                                     productUrl.StartsWith("/"))
                            {
                                productUrl =
                                    "https://www.alibaba.com" +
                                    productUrl;
                            }

                            // Only add products that have a name
                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                results.Add(
                                    new StoreProductResult
                                    {
                                        Name = name,

                                        Category = "General",

                                        Price = price,

                                        PreviousPrice =
                                            string.Empty,

                                        Discount =
                                            string.Empty,

                                        StoreName =
                                            "Alibaba",

                                        StoreLogoUrl =
                                            "/images/logos/alibaba_logo.svg.png",

                                        ImageUrl =
                                            imageUrl ?? string.Empty,

                                        ProductUrl =
                                            productUrl ?? string.Empty,

                                        Availability =
                                            "Available",

                                        SellerName =
                                            string.Empty
                                    }
                                );
                            }

                            // We only need 20 Alibaba products
                            if (results.Count >= 20)
                            {
                                break;
                            }
                        }
                        catch
                        {
                            // Skip products that cannot be read
                        }
                    }
                }
                catch (Exception)
                {
                    // If Alibaba cannot be accessed
                    // or its page structure changes,
                    // return an empty result.
                }
            }

            await Task.CompletedTask;

            return results;
        }
    }
}