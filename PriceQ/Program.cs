using Microsoft.EntityFrameworkCore;
using PriceQ.Data;
using PriceQ.Services;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. DATABASE CONNECTION
// ============================================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found."
    );

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
);

// ============================================================
// 2. IDENTITY
// ============================================================

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// ============================================================
// 3. MVC + RAZOR PAGES
// ============================================================

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ============================================================
// 4. STORE SEARCH SERVICES
// ============================================================

// Individual store services
builder.Services.AddScoped<DarazService>();
builder.Services.AddScoped<AmazonService>();
builder.Services.AddScoped<FlipkartService>();
builder.Services.AddScoped<HamroBazarService>();
builder.Services.AddScoped<AlibabaService>();

// Register every store through IStoreSearchService
builder.Services.AddScoped<IStoreSearchService, DarazService>();
builder.Services.AddScoped<IStoreSearchService, AmazonService>();
builder.Services.AddScoped<IStoreSearchService, FlipkartService>();
builder.Services.AddScoped<IStoreSearchService, HamroBazarService>();
builder.Services.AddScoped<IStoreSearchService, AlibabaService>();

// Main service which combines all stores
builder.Services.AddScoped<StoreSearchService>();

// ============================================================
// 5. PRICE SCRAPER
// ============================================================

builder.Services.AddScoped<PriceScraperService>();


// ============================================================
// BUILD APPLICATION
// ============================================================

var app = builder.Build();

// ============================================================
// HTTP REQUEST PIPELINE
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Authentication before Authorization
app.UseAuthentication();
app.UseAuthorization();

// MVC routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Identity Razor Pages
app.MapRazorPages();


// ============================================================
// DATABASE SEED DATA
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context =
        services.GetRequiredService<ApplicationDbContext>();

    // Create database if it doesn't exist
    context.Database.EnsureCreated();


    // ========================================================
    // CREATE INITIAL NIKE PRODUCT
    // ========================================================

    if (!context.Products.Any())
    {
        var product = new PriceQ.Models.Product
        {
            Name = "Nike Air Force 1",

            Category = "Shoes",

            CurrentPrice = "Rs. 12,299",

            PreviousPrice = "Rs. 15,000",

            Discount = "-16%",

            StoreName = "Daraz",

            ImageUrl = "/images/nike.png",

            StoreLogoUrl =
                "/images/logos/daraz_logo.svg.png",

            StoreUrl =
                "https://www.daraz.com.np/products/nike-air-force-1-07-shoes-for-men-white-i129130455.html"
        };

        context.Products.Add(product);

        context.SaveChanges();


        // ====================================================
        // CREATE DARAZ OFFER
        // ====================================================

        var darazOffer = new PriceQ.Models.ProductOffer
        {
            ProductId = product.Id,

            StoreName = "Daraz",

            StoreLogoUrl =
                "/images/logos/daraz_logo.svg.png",

            Price = "Rs. 12,299",

            PreviousPrice = "Rs. 15,000",

            Discount = "-16%",

            ProductUrl =
                "https://www.daraz.com.np/products/nike-air-force-1-07-shoes-for-men-white-i129130455.html",

            ImageUrl = "/images/nike.png",

            Availability = "Available",

            SellerName = "Daraz Seller",

            LastUpdated = DateTime.UtcNow
        };

        context.ProductOffers.Add(darazOffer);

        context.SaveChanges();
    }


    // ========================================================
    // MAKE SURE NIKE PRODUCT HAS DARAZ OFFER
    // ========================================================

    var nikeProduct =
        context.Products.FirstOrDefault(
            p => p.Name == "Nike Air Force 1"
        );

    if (nikeProduct != null)
    {
        var existingDarazOffer =
            context.ProductOffers.FirstOrDefault(
                o =>
                    o.ProductId == nikeProduct.Id &&
                    o.StoreName == "Daraz"
            );

        if (existingDarazOffer == null)
        {
            var darazOffer =
                new PriceQ.Models.ProductOffer
                {
                    ProductId = nikeProduct.Id,

                    StoreName = "Daraz",

                    StoreLogoUrl =
                        "/images/logos/daraz_logo.svg.png",

                    Price =
                        nikeProduct.CurrentPrice,

                    PreviousPrice =
                        nikeProduct.PreviousPrice,

                    Discount =
                        nikeProduct.Discount,

                    ProductUrl =
                        nikeProduct.StoreUrl,

                    ImageUrl =
                        nikeProduct.ImageUrl,

                    Availability = "Available",

                    SellerName = "Daraz Seller",

                    LastUpdated = DateTime.UtcNow
                };

            context.ProductOffers.Add(darazOffer);

            context.SaveChanges();
        }
    }
}


// ============================================================
// RUN APPLICATION
// ============================================================

app.Run();