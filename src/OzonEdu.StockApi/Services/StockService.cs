using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OzonEdu.StockApi.Models;
using OzonEdu.StockApi.Services.Interfaces;

namespace OzonEdu.StockApi.Services
{
    public class StockService : IStockService
    {
        private int _id = 0;
        private readonly ConcurrentBag<StockItem> _stockItems = new ConcurrentBag<StockItem>();

        public StockService()
        {
            _stockItems.Add( new StockItem(Interlocked.Increment(ref _id), "Футболка", 10));
            _stockItems.Add( new StockItem(Interlocked.Increment(ref _id), "Толстовка", 20));
            _stockItems.Add( new StockItem(Interlocked.Increment(ref _id), "Кепка", 15));
        }

        public Task<List<StockItem>> GetAll(CancellationToken _) => Task.FromResult(_stockItems.ToList());

        public Task<StockItem> GetById(long itemId, CancellationToken _)
        {
            var stockItem = _stockItems.FirstOrDefault(x => x.ItemId == itemId);
            return Task.FromResult(stockItem);
        }

        public Task<StockItem> Add(StockItemCreationModel stockItem, CancellationToken _)
        {
            var newStockItem = new StockItem(Interlocked.Increment(ref _id), stockItem.ItemName, stockItem.Quantity);
            _stockItems.Add(newStockItem);
            return Task.FromResult(newStockItem);
        }
    }
}