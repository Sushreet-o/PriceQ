using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PriceQ.Models;
using System.Diagnostics;
using PriceQ.Data;
using Microsoft.EntityFrameworkCore;
using PriceQ.Services;

namespace PriceQ.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly StoreSearchService _storeSearchService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            StoreSearchService storeSearchService,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _storeSearchService = storeSearchService;
            _userManager = userManager;
        }

        // ============================================================
        // HOME PAGE
        // ============================================================

        public IActionResult Index()
        {
            var todayDeals = new List<Product>
            {
                new Product
                {
                    Name = "iPhone 15 (128GB)",
                    CurrentPrice = "Rs. 104,999",
                    PreviousPrice = "Rs. 109,999",
                    StoreName = "Daraz",
                    StoreLogoUrl = "/images/logos/daraz_logo.svg.png",
                    ImageUrl = "/images/iphone15.png.png"
                },

                new Product
                {
                    Name = "MacBook Air M2",
                    CurrentPrice = "Rs. 134,999",
                    PreviousPrice = "Rs. 139,999",
                    StoreName = "Alibaba",
                    StoreLogoUrl = "/images/logos/alibaba_logo.svg.png",
                    ImageUrl = "/images/MacBook.png"
                },

                new Product
                {
                    Name = "JBL Wave 200TWS",
                    CurrentPrice = "Rs. 3,499",
                    PreviousPrice = "Rs. 3,999",
                    StoreName = "HamroBazar",
                    StoreLogoUrl = "/images/logos/hamrobazar_logo.svg.png",
                    ImageUrl = "/images/jbl.png"
                },

                new Product
                {
                    Name = "Noise ColorFit Pro 4",
                    CurrentPrice = "Rs. 5,199",
                    PreviousPrice = "Rs. 5,499",
                    StoreName = "Amazon",
                    StoreLogoUrl = "/images/logos/amazon_logo.svg.png",
                    ImageUrl = "/images/noise_watch.png.png"
                }
            };

            var topDealsRefined = new List<Product>
            {
                new Product
                {
                    Name = "Google Pixel 10 Pro XL",
                    CurrentPrice = "Rs. 200,000",
                    PreviousPrice = "Rs. 219,999",
                    StoreName = "Oliz Store",
                    StoreLogoUrl = "/images/logos/oliz_logo.png",
                    ImageUrl = "/images/google_pixel.png"
                },

                new Product
                {
                    Name = "Vans Skate",
                    CurrentPrice = "Rs. 9,000",
                    PreviousPrice = "Rs. 10,000",
                    StoreName = "Flipkart",
                    StoreLogoUrl = "/images/logos/Flipkart_logo.svg.png",
                    ImageUrl = "/images/vans.png"
                },

                new Product
                {
                    Name = "Logitech Zone Vibe",
                    CurrentPrice = "Rs. 19,999",
                    PreviousPrice = "Rs. 22,000",
                    StoreName = "Amazon",
                    StoreLogoUrl = "/images/logos/amazon_logo.svg.png",
                    ImageUrl = "/images/logitech.png"
                },

                new Product
                {
                    Name = "Ipad pro M5",
                    CurrentPrice = "Rs. 232,000",
                    PreviousPrice = "Rs. 250,000",
                    StoreName = "Oliz Store",
                    StoreLogoUrl = "/images/logos/oliz_logo.png",
                    ImageUrl = "/images/ipad.png"
                },

                new Product
                {
                    Name = "Lenovo Ideapad slim 3",
                    CurrentPrice = "Rs. 100,000",
                    PreviousPrice = "Rs. 139,999",
                    StoreName = "Hukut",
                    StoreLogoUrl = "/images/logos/hukut_logo.png",
                    ImageUrl = "/images/lenovo.png"
                },

                new Product
                {
                    Name = "Titan Blacksquare",
                    CurrentPrice = "Rs. 6,000",
                    PreviousPrice = "Rs. 7,999",
                    StoreName = "Titan",
                    StoreLogoUrl = "/images/logos/titan_logo.png",
                    ImageUrl = "/images/titan.png"
                }
            };

            ViewBag.TodayDealsList = todayDeals;

            return View(topDealsRefined);
        }

        // ============================================================
        // PRODUCT DETAILS
        // ============================================================

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Offers)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // ============================================================
        // SEARCH ALL STORES
        // ============================================================

        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }

            query = query.Trim();

            var results = new List<Product>();

            try
            {
                // ========================================================
                // SEARCH ALL REGISTERED STORES
                // ========================================================

                var storeResults =
                    await _storeSearchService.SearchAllStoresAsync(query);

                // ========================================================
                // CONVERT STORE RESULTS TO PRODUCT OBJECTS
                // ========================================================

                foreach (var item in storeResults)
                {
                    if (string.IsNullOrWhiteSpace(item.Name))
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(item.ProductUrl))
                    {
                        continue;
                    }

                    // ----------------------------------------------------
                    // Check whether this exact store product exists
                    // ----------------------------------------------------

                    var existingProduct = await _context.Products
                        .Include(p => p.Offers)
                        .FirstOrDefaultAsync(p =>
                            p.StoreUrl == item.ProductUrl);

                    // ====================================================
                    // PRODUCT DOES NOT EXIST
                    // ====================================================

                    if (existingProduct == null)
                    {
                        var newProduct = new Product
                        {
                            Name = item.Name,
                            Category = item.Category,
                            CurrentPrice = item.Price,
                            PreviousPrice = item.PreviousPrice,
                            Discount = item.Discount,
                            StoreName = item.StoreName,
                            StoreLogoUrl = GetStoreLogo(item.StoreName),
                            ImageUrl = item.ImageUrl,
                            StoreUrl = item.ProductUrl
                        };

                        _context.Products.Add(newProduct);

                        await _context.SaveChangesAsync();

                        // Create offer
                        var offer = new ProductOffer
                        {
                            ProductId = newProduct.Id,
                            StoreName = item.StoreName,
                            StoreLogoUrl = GetStoreLogo(item.StoreName),
                            Price = item.Price,
                            PreviousPrice = item.PreviousPrice,
                            Discount = item.Discount,
                            ProductUrl = item.ProductUrl,
                            ImageUrl = item.ImageUrl,
                            Availability = item.Availability,
                            SellerName = item.SellerName,
                            LastUpdated = DateTime.UtcNow
                        };

                        _context.ProductOffers.Add(offer);

                        await _context.SaveChangesAsync();

                        results.Add(newProduct);
                    }

                    // ====================================================
                    // PRODUCT ALREADY EXISTS
                    // ====================================================

                    else
                    {
                        existingProduct.Name = item.Name;
                        existingProduct.Category = item.Category;
                        existingProduct.CurrentPrice = item.Price;
                        existingProduct.PreviousPrice = item.PreviousPrice;
                        existingProduct.Discount = item.Discount;
                        existingProduct.StoreName = item.StoreName;
                        existingProduct.StoreLogoUrl =
                            GetStoreLogo(item.StoreName);
                        existingProduct.ImageUrl = item.ImageUrl;
                        existingProduct.StoreUrl = item.ProductUrl;

                        // ------------------------------------------------
                        // Find offer for this store
                        // ------------------------------------------------

                        var existingOffer =
                            existingProduct.Offers
                                .FirstOrDefault(o =>
                                    o.StoreName == item.StoreName);

                        if (existingOffer == null)
                        {
                            existingOffer = new ProductOffer
                            {
                                ProductId = existingProduct.Id,
                                StoreName = item.StoreName,
                                StoreLogoUrl =
                                    GetStoreLogo(item.StoreName),
                                Price = item.Price,
                                PreviousPrice = item.PreviousPrice,
                                Discount = item.Discount,
                                ProductUrl = item.ProductUrl,
                                ImageUrl = item.ImageUrl,
                                Availability = item.Availability,
                                SellerName = item.SellerName,
                                LastUpdated = DateTime.UtcNow
                            };

                            _context.ProductOffers.Add(existingOffer);
                        }
                        else
                        {
                            existingOffer.StoreLogoUrl =
                                GetStoreLogo(item.StoreName);

                            existingOffer.Price = item.Price;
                            existingOffer.PreviousPrice =
                                item.PreviousPrice;
                            existingOffer.Discount = item.Discount;
                            existingOffer.ImageUrl = item.ImageUrl;
                            existingOffer.ProductUrl = item.ProductUrl;
                            existingOffer.Availability =
                                item.Availability;
                            existingOffer.SellerName =
                                item.SellerName;
                            existingOffer.LastUpdated =
                                DateTime.UtcNow;
                        }

                        await _context.SaveChangesAsync();

                        if (!results.Any(p => p.Id == existingProduct.Id))
                        {
                            results.Add(existingProduct);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while searching all stores for {Query}",
                    query);
            }

            // ============================================================
            // MAXIMUM 20 PRODUCTS
            // ============================================================

            results = results
                .Take(20)
                .ToList();

            ViewData["SearchQuery"] = query;

            return View(results);
        }

        // ============================================================
        // STORE LOGO HELPER
        // ============================================================

        private string GetStoreLogo(string storeName)
        {
            if (string.IsNullOrWhiteSpace(storeName))
            {
                return "/images/logos/default_store.png";
            }

            switch (storeName.Trim().ToLower())
            {
                case "daraz":
                    return "/images/logos/daraz_logo.svg.png";

                case "amazon":
                    return "/images/logos/amazon_logo.svg.png";

                case "flipkart":
                    return "/images/logos/Flipkart_logo.svg.png";

                case "hamrobazar":
                case "hamrobazaar":
                    return "/images/logos/hamrobazar_logo.svg.png";

                case "alibaba":
                    return "/images/logos/alibaba_logo.svg.png";

                default:
                    return "/images/logos/default_store.png";
            }
        }

        // ============================================================
        // REFRESH PRICE
        // ============================================================

        [HttpPost]
        public async Task<IActionResult> RefreshPrice(int id)
        {
            var product = await _context.Products
                .Include(p => p.Offers)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null &&
                !string.IsNullOrEmpty(product.StoreUrl))
            {
                try
                {
                    var scraper =
                        new PriceQ.Services.PriceScraperService();

                    string livePrice =
                        scraper.GetDarazPrice(product.StoreUrl);

                    if (livePrice != "Price Unavailable")
                    {
                        product.CurrentPrice = livePrice;

                        var darazOffer =
                            product.Offers.FirstOrDefault(o =>
                                o.StoreName == "Daraz");

                        if (darazOffer != null)
                        {
                            darazOffer.Price = livePrice;
                            darazOffer.LastUpdated =
                                DateTime.UtcNow;
                        }

                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error refreshing price for product {Id}",
                        id);
                }
            }

            return RedirectToAction(
                "Details",
                new { id = id });
        }

        // ============================================================
        // TEST ALL STORES
        // ============================================================

        public async Task<IActionResult> TestStores(
            string query = "Nike")
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                query = "Nike";
            }

            try
            {
                var results =
                    await _storeSearchService.SearchAllStoresAsync(query);

                return Json(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Store search failed for {Query}",
                    query);

                return StatusCode(
                    500,
                    new
                    {
                        error = "Store search failed.",
                        message = ex.Message
                    });
            }
        }

        // ============================================================
        // TEST AMAZON
        // ============================================================

        public async Task<IActionResult> TestAmazon(
            string query = "Nike")
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                query = "Nike";
            }

            try
            {
                var amazonService =
                    HttpContext.RequestServices
                        .GetRequiredService<AmazonService>();

                var results =
                    await amazonService.SearchAsync(query);

                return Json(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Amazon test search failed for {Query}",
                    query);

                return StatusCode(
                    500,
                    new
                    {
                        error = "Amazon search failed.",
                        message = ex.Message
                    });
            }
        }

        // ============================================================
        // STORES
        // ============================================================

        public IActionResult Stores()
        {
            var stores = new List<Store>
            {
                new Store
                {
                    Name = "Daraz",
                    Description =
                        "Nepal's largest online marketplace",
                    LogoUrl =
                        "/images/logos/daraz_logo.svg.png",
                    Rating = 4.5,
                    ReviewCount = "25.6k",
                    Url = "https://www.daraz.com.np"
                },

                new Store
                {
                    Name = "Alibaba",
                    Description =
                        "Global B2B wholesale platform",
                    LogoUrl =
                        "/images/logos/alibaba_logo.svg.png",
                    Rating = 4.6,
                    ReviewCount = "100k+",
                    Url = "https://www.alibaba.com"
                },

                new Store
                {
                    Name = "HamroBazar",
                    Description =
                        "Buy and sell with trusted sellers",
                    LogoUrl =
                        "/images/logos/hamrobazar_logo.svg.png",
                    Rating = 4.2,
                    ReviewCount = "6.1k",
                    Url = "https://hamrobazaar.com"
                },

                new Store
                {
                    Name = "Amazon",
                    Description =
                        "Global products, fast delivery",
                    LogoUrl =
                        "/images/logos/amazon_logo.svg.png",
                    Rating = 4.4,
                    ReviewCount = "12.8k",
                    Url = "https://www.amazon.com"
                },

                new Store
                {
                    Name = "Flipkart",
                    Description =
                        "Best of electronics and fashion",
                    LogoUrl =
                        "/images/logos/flipkart_logo.svg.png",
                    Rating = 4.3,
                    ReviewCount = "7.2k",
                    Url = "https://www.flipkart.com"
                }
            };

            return View(stores);
        }

        // ============================================================
        // BLOG
        // ============================================================

        public IActionResult Blog()
        {
            var posts = new List<BlogPost>
            {
                new BlogPost
                {
                    Title =
                        "Top 5 Budget Smartphones in Nepal under Rs. 30,000",
                    Excerpt =
                        "Looking for a new phone? We compared prices across all major Nepali stores to find the winners.",
                    Category = "Electronics",
                    Author = "PriceQ Team",
                    Date = "April 28, 2026",
                    ImageUrl =
                        "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?q=80&w=500"
                },

                new BlogPost
                {
                    Title =
                        "How to use Price Alerts to Save Money on the iPhone 15",
                    Excerpt =
                        "A step-by-step guide to setting up your first alert and catching the next big price drop.",
                    Category = "Shopping Tips",
                    Author = "Admin",
                    Date = "April 25, 2026",
                    ImageUrl =
                        "https://images.unsplash.com/photo-1592899677977-9c10ca588bbd?q=80&w=500"
                },

                new BlogPost
                {
                    Title =
                        "Sneaker Trends: Best Footwear Deals this Summer",
                    Excerpt =
                        "From Nike to local brands, here is where to find the best deals on shoes in Kathmandu.",
                    Category = "Fashion",
                    Author = "Style Guide",
                    Date = "April 20, 2026",
                    ImageUrl =
                        "https://images.unsplash.com/photo-1542291026-7eec264c27ff?q=80&w=500"
                }
            };

            return View(posts);
        }

        // ============================================================
        // DEALS
        // ============================================================

        public IActionResult Deals()
        {
            var allDeals = new List<Product>
            {
                new Product
                {
                    Name = "IPhone 17",
                    Category = "Smartphones",
                    CurrentPrice = "Rs. 167,999",
                    PreviousPrice = "Rs. 169,999",
                    Discount = "-15%",
                    StoreName = "Hukut",
                    StoreLogoUrl =
                        "/images/logos/hukut_logo.png",
                    ImageUrl = "/images/iphone17.png"
                },

                new Product
                {
                    Name = "Marshall Speaker",
                    Category = "Speaker",
                    CurrentPrice = "Rs. 25,000",
                    PreviousPrice = "Rs. 28,000",
                    Discount = "-20%",
                    StoreName = "Oliz Store",
                    StoreLogoUrl =
                        "/images/logos/oliz_logo.png",
                    ImageUrl = "/images/marshall.png"
                },

                new Product
                {
                    Name = "Jacket",
                    Category = "Men's Fashion",
                    CurrentPrice = "Rs. 2,499",
                    PreviousPrice = "Rs. 3,699",
                    Discount = "-25%",
                    StoreName = "Daraz",
                    StoreLogoUrl =
                        "/images/logos/daraz_logo.svg.png",
                    ImageUrl = "/images/jacket.png"
                },

                new Product
                {
                    Name = "Noise ColorFit Pro 4",
                    Category = "Smartwatch",
                    CurrentPrice = "Rs. 5,199",
                    PreviousPrice = "Rs. 7,499",
                    Discount = "-30%",
                    StoreName = "HamroBazar",
                    StoreLogoUrl =
                        "/images/logos/hamrobazar_logo.svg.png",
                    ImageUrl = "/images/noise_watch.png.png"
                },

                new Product
                {
                    Name = "Temperature Water Bottle",
                    Category = "Drinkware",
                    CurrentPrice = "Rs. 2,000",
                    PreviousPrice = "Rs. 2,500",
                    Discount = "-10%",
                    StoreName = "Daraz",
                    StoreLogoUrl =
                        "/images/logos/daraz_logo.svg.png",
                    ImageUrl = "/images/waterbottle.png"
                },

                new Product
                {
                    Name = "Hooded Sweatshirt",
                    Category = "Men's Fashion",
                    CurrentPrice = "Rs. 1,999",
                    PreviousPrice = "Rs. 2,450",
                    Discount = "-18%",
                    StoreName = "Daraz",
                    StoreLogoUrl =
                        "/images/logos/daraz_logo.svg.png",
                    ImageUrl = "/images/hoodie.png"
                },

                new Product
                {
                    Name = "Wild Stone Perfume",
                    Category = "Beauty",
                    CurrentPrice = "Rs. 1,300",
                    PreviousPrice = "Rs. 1,480",
                    Discount = "-22%",
                    StoreName = "Flipkart",
                    StoreLogoUrl =
                        "/images/logos/Flipkart_logo.svg.png",
                    ImageUrl = "/images/wildstone.png"
                },

                new Product
                {
                    Name = "Sony WH-CH720N",
                    Category = "Headphones",
                    CurrentPrice = "Rs. 12,499",
                    PreviousPrice = "Rs. 19,299",
                    Discount = "-35%",
                    StoreName = "Amazon",
                    StoreLogoUrl =
                        "/images/logos/amazon_logo.svg.png",
                    ImageUrl = "/images/sony.png"
                }
            };

            return View(allDeals);
        }

        // ============================================================
        // PRIVACY
        // ============================================================

        public IActionResult Privacy()
        {
            return View();
        }

        // ============================================================
        // ERROR
        // ============================================================

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId =
                        Activity.Current?.Id ??
                        HttpContext.TraceIdentifier
                });
        }

        // ============================================================
        // ALERTS
        // ============================================================

        public IActionResult Alerts()
        {
            var alerts = new List<PriceAlert>
            {
                new PriceAlert
                {
                    ProductName = "Casio EDIFICE",
                    ProductImageUrl = "/images/casio.png",
                    StoreName = "Daraz",
                    CurrentPrice = "Rs. 28,000",
                    TargetPrice = "Rs. 25,000",
                    IsActive = true
                },

                new PriceAlert
                {
                    ProductName = "Converse",
                    ProductImageUrl = "/images/converse.png",
                    StoreName = "Daraz",
                    CurrentPrice = "Rs. 10,000",
                    TargetPrice = "Rs. 8,000",
                    IsActive = true
                },

                new PriceAlert
                {
                    ProductName = "Anker Powerbank",
                    ProductImageUrl = "/images/anker.png",
                    StoreName = "Hukut",
                    CurrentPrice = "Rs. 18,000",
                    TargetPrice = "Rs. 15,000",
                    IsActive = true
                },

                new PriceAlert
                {
                    ProductName = "JBL Wave 200TWS",
                    ProductImageUrl = "/images/jbl.png",
                    StoreName = "HamroBazar",
                    CurrentPrice = "Rs. 3,499",
                    TargetPrice = "Rs. 3,000",
                    IsActive = true
                }
            };

            return View(alerts);
        }

        // ============================================================
        // SAVED
        // ============================================================

        public IActionResult Saved()
        {
            var savedItems = new List<Product>
            {
                new Product
                {
                    Name = "iPhone 15 (128GB)",
                    CurrentPrice = "Rs. 104,999",
                    StoreName = "Daraz",
                    ImageUrl = "/images/iphone15.png.png"
                },

                new Product
                {
                    Name = "Nike Air Force 1",
                    CurrentPrice = "Rs. 12,299",
                    StoreName = "Nike Nepal",
                    ImageUrl = "/images/nike_af1.png.png"
                }
            };

            return View(savedItems);
        }

        // ============================================================
        // PROFILE - VIEW
        // ============================================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? ""
            };

            return View(model);
        }


        // ============================================================
        // PROFILE - SAVE CHANGES
        // ============================================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            // Update profile information
            user.FirstName = model.FirstName?.Trim();
            user.LastName = model.LastName?.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["ProfileSuccess"] =
                    "Your profile has been updated successfully.";

                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
    }
}