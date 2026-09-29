namespace PriceQ.Models
{
    public class ProductOffer
    {
        public int Id { get; set; }

        // The product this offer belongs to
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Store information
        public string StoreName { get; set; } = string.Empty;
        public string StoreLogoUrl { get; set; } = string.Empty;

        // Information from the store
        public string Price { get; set; } = string.Empty;
        public string PreviousPrice { get; set; } = string.Empty;
        public string Discount { get; set; } = string.Empty;

        // Product information from the store
        public string ProductUrl { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        // Availability
        public string Availability { get; set; } = string.Empty;

        // Seller information, useful for marketplaces
        public string SellerName { get; set; } = string.Empty;

        // When PriceQ last checked this offer
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}