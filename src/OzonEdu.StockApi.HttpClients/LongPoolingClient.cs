using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OzonEdu.StockApi.HttpModels;

namespace OzonEdu.StockApi.HttpClients
{
    public class LongPoolingClient : ILongPoolingClient
    {
        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions _defaultOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public LongPoolingClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<V1AddResponse> Add(CancellationToken token)
        {
            using var response = await _httpClient.PostAsync("v1/api/long-pooling/add", new StringContent(string.Empty), token);
            var body = await response.Content.ReadAsStringAsync(token);
            return JsonSerializer.Deserialize<V1AddResponse>(body, _defaultOptions);
        }

        public async Task<V1CheckIfProcessedResponse> CheckIfProcessedRequest(V1CheckIfProcessedRequest request, CancellationToken token)
        {
            using var response = await _httpClient.PostAsync("v1/api/long-pooling/check-if-processed", JsonContent.Create(request), token);
            var body = await response.Content.ReadAsStringAsync(token);
            return JsonSerializer.Deserialize<V1CheckIfProcessedResponse>(body, _defaultOptions);
        }
    }
}