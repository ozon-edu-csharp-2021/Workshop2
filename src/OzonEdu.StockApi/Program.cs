using System;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using OzonEdu.StockApi;
using OzonEdu.StockApi.Infrastructure.Extensions;

CreateHostBuilder(args).Build().Run();

static IHostBuilder CreateHostBuilder(string[] args)
    => Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseShutdownTimeout(TimeSpan.FromSeconds(5));
            webBuilder.UseStartup<Startup>();
        })
        .AddInfrastructure()
        .AddHttp();