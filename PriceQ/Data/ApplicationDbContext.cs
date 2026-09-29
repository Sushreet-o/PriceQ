using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PriceQ.Models; // Ensure this is added to find the Product class

namespace PriceQ.Data
{
    // Inheriting from IdentityDbContext gives us all the User/Login tables for free
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // This line creates the Products table in your database
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductOffer> ProductOffers { get; set; }
    }
}