using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using PriceQ.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PriceQ.Services
{
    public class AmazonService : IStoreSearchService
    {
        public string StoreName => "Amazon";

        public async Task<List<StoreProductResult>> SearchAsync(string searchQuery)
        {
            var results = new List<StoreProductResult>();

            var options = new ChromeOptions();

            options.AddArgument("--headless=new");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--window-size=1920,1080");

            // Helps Amazon treat the browser more like a normal browser
            options.AddArgument(
                "--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/131.0.0.0 Safari/537.36"
            );

            using (IWebDriver driver = new ChromeDriver(options))
            {
                try
                {
                    string searchUrl =
                        "https://www.amazon.com/s?k=" +
                        Uri.EscapeDataString(searchQuery);

                    driver.Navigate().GoToUrl(searchUrl);

                    // Give Amazon some time to load
                    await Task.Delay(3000);

                    var wait = new WebDriverWait(
                        driver,
                        TimeSpan.FromSeconds(20)
                    );

                    // Wait for Amazon search results
                    wait.Until(d =>
                        d.FindElements(
                            By.CssSelector(
                                "div[data-component-type='s-search-result']"
                            )
                        ).Count > 0
                    );

                    var products = driver.FindElements(
                        By.CssSelector(
                            "div[data-component-type='s-search-result']"
                        )
                    );

                    foreach (var product in products)
                    {
                        if (results.Count >= 10)
                            break;

                        try
                        {
                            // ============================================
                            // PRODUCT NAME
                            // ============================================

                            var nameElements = product.FindElements(
                                By.CssSelector("h2 span")
                            );

                            if (nameElements.Count == 0)
                                continue;

                            string name = nameElements[0].Text.Trim();

                            if (string.IsNullOrWhiteSpace(name))
                                continue;


                            // ============================================
                            // PRICE
                            // ============================================

                            var priceElements = product.FindElements(
                                By.CssSelector(
                                    "span.a-price span.a-offscreen"
                                )
                            );

                            string price = "Price unavailable";

                            if (priceElements.Count > 0)
                            {
                                price =
                                    priceElements[0].Text.Trim();
                            }


                            // ============================================
                            // PRODUCT URL
                            // ============================================

                            var linkElements = product.FindElements(
                                By.CssSelector("h2 a")
                            );

                            if (linkElements.Count == 0)
                                continue;

                            string productUrl =
                                linkElements[0]
                                    .GetAttribute("href") ?? "";

                            if (string.IsNullOrWhiteSpace(productUrl))
                                continue;

                            if (productUrl.StartsWith("/"))
                            {
                                productUrl =
                                    "https://www.amazon.com" +
                                    productUrl;
                            }


                            // ============================================
                            // PRODUCT IMAGE
                            // ============================================

                            var imageElements = product.FindElements(
                                By.CssSelector("img.s-image")
                            );

                            string imageUrl = "";

                            if (imageElements.Count > 0)
                            {
                                imageUrl =
                                    imageElements[0]
                                        .GetAttribute("src") ?? "";
                            }

                            // Some Amazon images use data-src
                            if (string.IsNullOrWhiteSpace(imageUrl) &&
                                imageElements.Count > 0)
                            {
                                imageUrl =
                                    imageElements[0]
                                        .GetAttribute("data-src") ?? "";
                            }


                            // ============================================
                            // DISCOUNT / OLD PRICE
                            // ============================================

                            var oldPriceElements =
                                product.FindElements(
                                    By.CssSelector(
                                        "span.a-price.a-text-price " +
                                        "span.a-offscreen"
                                    )
                                );

                            string previousPrice = "";

                            if (oldPriceElements.Count > 0)
                            {
                                previousPrice =
                                    oldPriceElements[0]
                                        .Text.Trim();
                            }


                            // ============================================
                            // ADD RESULT
                            // ============================================

                            results.Add(
                                new StoreProductResult
                                {
                                    Name = name,

                                    Category = "General",

                                    Price = price,

                                    PreviousPrice =
                                        previousPrice,

                                    Discount = "",

                                    StoreName = "Amazon",

                                    StoreLogoUrl =
                                        "/images/logos/amazon_logo.svg.png",

                                    ImageUrl =
                                        imageUrl,

                                    ProductUrl =
                                        productUrl,

                                    Availability =
                                        "Available",

                                    SellerName =
                                        ""
                                }
                            );
                        }
                        catch
                        {
                            // Skip individual products
                            // that cannot be read.
                        }
                    }
                }
                catch (Exception ex)
                {
                    // IMPORTANT:
                    // Do NOT silently return [].
                    // This tells us exactly what Amazon is doing.

                    Console.WriteLine(
                        "===================================="
                    );

                    Console.WriteLine(
                        "AMAZON SEARCH ERROR"
                    );

                    Console.WriteLine(
                        ex.Message
                    );

                    Console.WriteLine(
                        "===================================="
                    );
                }
            }

            return results;
        }
    }
}