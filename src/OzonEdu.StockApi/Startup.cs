using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OzonEdu.StockApi.GrpcServices;
using OzonEdu.StockApi.Infrastructure.Interceptors;
using OzonEdu.StockApi.Services;
using OzonEdu.StockApi.Services.Interfaces;
using Quartz;

namespace OzonEdu.StockApi
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IStockService, StockService>();
            services.AddScoped<ILoggingService, LoggingService>();
            services.AddSingleton<ILongPoolingService, LongPoolingService>();

            services.AddGrpc(options => options.Interceptors.Add<LoggingInterceptor>());
            services.AddHostedService<CustomHostedService>();
            services.AddQuartzHostedService();
            services.AddTransient<CustomJob>();
            services.AddQuartz(options =>
            {
                options.UseMicrosoftDependencyInjectionJobFactory();
                options.ScheduleJob<CustomJob>(trigger => trigger.StartNow()
                    .WithIdentity(nameof(CustomJob))
                    //.WithCronSchedule("0 0 0/4 ? * 2/2 *")
                    .WithSimpleSchedule(schedule => schedule.WithInterval(TimeSpan.FromSeconds(2))
                        ));
                
                options.AddJobListener<CustomJobListerner>();
            });
            
            services.AddQuartzServer(options => options.WaitForJobsToComplete = true);
        }

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
			app.UseRouting();
			app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<StockApiGrpService>();
                endpoints.MapControllers();
            });
		}
	}

    public class CustomJobListerner : IJobListener
    {
        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException,
            CancellationToken cancellationToken = new CancellationToken())
        {
            Console.WriteLine($"Job {context.JobDetail.JobType.ToString()} finished");
            return Task.CompletedTask;
        }

        public string Name { get; } = "Custom job listener";
    }

    [DisallowConcurrentExecution]
    public class CustomJob : IJob
    {
        // etcd
        private readonly ILoggingService _loggingService;

        public CustomJob(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Delay(5000, context.CancellationToken);
            _loggingService.Log(DateTimeOffset.Now.ToString());
            _loggingService.Log($"{nameof(CustomJob)} tick");
        }
    }
    
    // ....
    public interface ILoggingService
    {
        void Log(string message);
    }

    public class LoggingService : ILoggingService
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }

    public class CustomHostedService : IHostedService, IDisposable
    {
        private Timer _timer;
        //private readonly ILoggingService _loggingService;
        private readonly IServiceProvider _serviceProvider;

        public CustomHostedService(IServiceProvider serviceProvider)
        {
            //_loggingServicervice = loggingService;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var loggingService = scope.ServiceProvider.GetRequiredService<ILoggingService>();
            async void Callback(object _)
            {
                try
                {
                    await Task.Delay(5000, cancellationToken);
                    loggingService.Log(DateTimeOffset.Now.ToString());
                    loggingService.Log("IHostedService tick");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            _timer = new Timer(Callback, state: null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }

    public class CustomBackgroundService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("IHostedService tick");
                await Task.Delay(100, stoppingToken);
            }
        }
    }
}