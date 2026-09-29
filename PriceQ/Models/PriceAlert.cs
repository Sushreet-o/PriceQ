namespace PriceQ.Models
{
    public class PriceAlert
    {
        // 1. Link to the Product
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public string StoreName { get; set; }

        // 2. Alert Specific Data
        public string CurrentPrice { get; set; }
        public string TargetPrice { get; set; } // e.g., "Rs. 95,000"

        // 3. Status (True = On/Purple toggle, False = Off/Grey toggle)
        public bool IsActive { get; set; }
    }
}