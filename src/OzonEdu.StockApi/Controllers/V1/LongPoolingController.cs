using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OzonEdu.StockApi.HttpClients;
using OzonEdu.StockApi.HttpModels;
using OzonEdu.StockApi.Services.Interfaces;

namespace OzonEdu.StockApi.Controllers.V1
{
    [ApiController]
    [Route("v1/api/long-pooling")]
    public class LongPoolingController : ControllerBase
    {
        private readonly ILongPoolingService _longPoolingService;

        public LongPoolingController(ILongPoolingService longPoolingService)
        {
            _longPoolingService = longPoolingService;
        }

        [HttpPost("add")]
        public async Task<ActionResult<V1AddResponse>> Add(CancellationToken token)
        {
            var id = await _longPoolingService.Add(token);
            return Ok(new V1AddResponse {Guid = id});
        }

        [HttpPost("check-if-processed")]
        public async Task<ActionResult<V1CheckIfProcessedResponse>> CheckIfProcessed(V1CheckIfProcessedRequest request,
            CancellationToken token)
        {
            var result = await _longPoolingService.CheckIfProcessed(request.Guid, token);
            return Ok(new V1CheckIfProcessedResponse {IsProcessed = result});
        }

        [HttpPost("test-execute")]
        public async Task<IActionResult> Execute(CancellationToken token)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            var client = new LongPoolingClient(new HttpClient { BaseAddress = new Uri("http://localhost:5000")});
            var guidResponse = await client.Add(cts.Token);

            Console.WriteLine(guidResponse.Guid.ToString());

            while (!cts.Token.IsCancellationRequested)
            {
                Console.WriteLine("Tick");
                var statusResponse = await client.CheckIfProcessedRequest(new V1CheckIfProcessedRequest
                {
                    Guid = guidResponse.Guid
                }, cts.Token);
                if (statusResponse.IsProcessed)
                {
                    return Ok();
                }

                await Task.Delay(200, cts.Token);
            }

            return BadRequest();
        }
    }
}