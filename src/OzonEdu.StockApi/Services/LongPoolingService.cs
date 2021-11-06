using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using OzonEdu.StockApi.Services.Interfaces;

namespace OzonEdu.StockApi.Services
{
    public class LongPoolingService : ILongPoolingService
    {
        private const int Threshold = 10;
        private readonly ConcurrentDictionary<Guid, int> _requests = new ConcurrentDictionary<Guid, int>();

        public async Task<Guid> Add(CancellationToken token)
        {
            await Task.Delay(150, token);
            var guid = Guid.NewGuid();
            _requests.TryAdd(guid, 0);
            return guid;
        }

        public async Task<bool> CheckIfProcessed(Guid guid, CancellationToken token)
        {
            await Task.Delay(100, token);
            _requests[guid]++;
            return _requests[guid] > Threshold;
        }
    }
}