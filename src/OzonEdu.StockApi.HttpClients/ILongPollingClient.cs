using System.Threading;
using System.Threading.Tasks;
using OzonEdu.StockApi.HttpModels;

namespace OzonEdu.StockApi.HttpClients
{
    public interface ILongPollingClient
    {
        Task<V1AddResponse> Add(CancellationToken token);
        Task<V1CheckIfProcessedResponse> CheckIfProcessedRequest(V1CheckIfProcessedRequest request, CancellationToken token);
    }
}