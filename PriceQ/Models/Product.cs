namespace PriceQ.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; } // Added for filtering
        public string CurrentPrice { get; set; }
        public string PreviousPrice { get; set; }
        public string Discount { get; set; } // e.g., "-15%"
        public string StoreName { get; set; }
        public string StoreLogoUrl { get; set; }
        public string ImageUrl { get; set; }

        public string StoreUrl { get; set; } = string.Empty;

        // All store offers for this product
        public List<ProductOffer> Offers { get; set; } = new List<ProductOffer>();
    }
}