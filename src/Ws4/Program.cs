// See https://aka.ms/new-console-template for more information

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OzonEdu.StockApi.HttpClients;
using OzonEdu.StockApi.HttpModels;

using var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(3));

var client = new LongPoolingClient(new HttpClient { BaseAddress = new Uri("http://localhost:5000")});
var guidResponse = await client.Add(cts.Token);

Console.WriteLine(guidResponse.Guid.ToString());

while (!cts.Token.IsCancellationRequested)
{
    var statusResponse = await client.CheckIfProcessedRequest(new V1CheckIfProcessedRequest
    {
        Guid = guidResponse.Guid
    }, cts.Token);
    if (statusResponse.IsProcessed)
    {
        Console.WriteLine("Yay!");
        break;
    }

    await Task.Delay(200);
}