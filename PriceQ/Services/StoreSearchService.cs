using PriceQ.Models;

namespace PriceQ.Services
{
    public class StoreSearchService
    {
        private readonly IEnumerable<IStoreSearchService> _storeServices;

        public StoreSearchService(
            IEnumerable<IStoreSearchService> storeServices)
        {
            _storeServices = storeServices;
        }

        public async Task<List<StoreProductResult>> SearchAllStoresAsync(
            string searchQuery)
        {
            var allResults = new List<StoreProductResult>();

            var searchTasks = _storeServices
                .Select(async store =>
                {
                    try
                    {
                        var results =
                            await store.SearchAsync(searchQuery);

                        return new
                        {
                            Store = store.StoreName,
                            Results = results ?? new List<StoreProductResult>()
                        };
                    }
                    catch
                    {
                        return new
                        {
                            Store = store.StoreName,
                            Results = new List<StoreProductResult>()
                        };
                    }
                })
                .ToList();

            var storeResults = await Task.WhenAll(searchTasks);

            // ============================================================
            // TAKE UP TO 4 PRODUCTS FROM EACH STORE
            // ============================================================

            foreach (var store in storeResults)
            {
                var productsFromStore = store.Results
                    .Where(p =>
                        !string.IsNullOrWhiteSpace(p.Name) &&
                        !string.IsNullOrWhiteSpace(p.ProductUrl))
                    .Take(4)
                    .ToList();

                allResults.AddRange(productsFromStore);
            }

            // ============================================================
            // MAXIMUM 20 PRODUCTS
            // ============================================================

            return allResults
                .Take(20)
                .ToList();
        }
    }
}