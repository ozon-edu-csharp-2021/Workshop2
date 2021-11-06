using System;
using System.Threading;
using System.Threading.Tasks;

namespace OzonEdu.StockApi.Services.Interfaces
{
    public interface ILongPoolingService
    {
        Task<Guid> Add(CancellationToken token);
        
        Task<bool> CheckIfProcessed(Guid guid, CancellationToken token);
    }
}