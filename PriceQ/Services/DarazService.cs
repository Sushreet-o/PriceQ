using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using PriceQ.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PriceQ.Services
{
    public class DarazService : IStoreSearchService
    {
        public string StoreName => "Daraz";

        public async Task<List<StoreProductResult>> SearchAsync(string searchQuery)
        {
            var results = new List<StoreProductResult>();

            var options = new ChromeOptions();

            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            using (IWebDriver driver = new ChromeDriver(options))
            {
                try
                {
                    // Create Daraz search URL
                    string searchUrl =
                        "https://www.daraz.com.np/catalog/?q="
                        + Uri.EscapeDataString(searchQuery);

                    driver.Navigate().GoToUrl(searchUrl);

                    var wait = new WebDriverWait(
                        driver,
                        TimeSpan.FromSeconds(15)
                    );

                    // Wait for products to appear
                    wait.Until(driver =>
                        driver.FindElements(
                            By.CssSelector(
                                "div[data-qa-locator='product-item']")
                        ).Count > 0
                    );

                    var products = driver.FindElements(
                        By.CssSelector(
                            "div[data-qa-locator='product-item']")
                    );

                    foreach (var product in products)
                    {
                        try
                        {
                            // ------------------------------------------------
                            // PRODUCT NAME
                            // ------------------------------------------------

                            var nameElement = product.FindElement(
                                By.CssSelector("div.RfADt")
                            );

                            string name = nameElement.Text.Trim();


                            // ------------------------------------------------
                            // PRICE
                            // ------------------------------------------------

                            var priceElement = product.FindElement(
                                By.CssSelector("span.ooOxS")
                            );

                            string price = priceElement.Text.Trim();


                            // ------------------------------------------------
                            // PRODUCT LINK
                            // ------------------------------------------------

                            var linkElement = product.FindElement(
                                By.CssSelector("a")
                            );

                            string productUrl =
                                linkElement.GetAttribute("href");

                            if (!string.IsNullOrEmpty(productUrl) &&
                                productUrl.StartsWith("/"))
                            {
                                productUrl =
                                    "https://www.daraz.com.np"
                                    + productUrl;
                            }


                            // ------------------------------------------------
                            // PRODUCT IMAGE
                            // ------------------------------------------------

                            string imageUrl = string.Empty;

                            var imageElements =
                                product.FindElements(
                                    By.CssSelector("img")
                                );

                            if (imageElements.Count > 0)
                            {
                                var imageElement = imageElements[0];

                                // Daraz may use different attributes
                                // depending on lazy loading.

                                imageUrl =
                                    imageElement.GetAttribute("src");

                                if (string.IsNullOrWhiteSpace(imageUrl))
                                {
                                    imageUrl =
                                        imageElement.GetAttribute(
                                            "data-src");
                                }

                                if (string.IsNullOrWhiteSpace(imageUrl))
                                {
                                    imageUrl =
                                        imageElement.GetAttribute(
                                            "data-original");
                                }

                                if (string.IsNullOrWhiteSpace(imageUrl))
                                {
                                    imageUrl =
                                        imageElement.GetAttribute(
                                            "data-lazy-src");
                                }
                            }


                            // ------------------------------------------------
                            // CREATE RESULT
                            // ------------------------------------------------

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
                                        "Daraz",

                                    StoreLogoUrl =
                                        "/images/logos/daraz_logo.svg.png",

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
                        catch
                        {
                            // Skip products that don't contain
                            // all required information.
                        }
                    }
                }
                catch (Exception)
                {
                    // If Daraz cannot be accessed or the
                    // page structure changes, return empty results.
                }
            }

            await Task.CompletedTask;

            return results;
        }
    }
}