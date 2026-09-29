using PriceQ.Models;

namespace PriceQ.Services
{
    public interface IStoreSearchService
    {
        string StoreName { get; }

        Task<List<StoreProductResult>> SearchAsync(string searchQuery);
    }
}