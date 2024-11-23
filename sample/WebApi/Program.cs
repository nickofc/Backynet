using Backynet;
using Backynet.Options;
using WebApplication2;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();

builder.Services.AddBackynetContext<DefaultBackynetContext>((sp, options) =>
{
    options.UsePostgreSql(builder.Configuration.GetConnectionString("PostgreSql") ??
                          throw new InvalidOperationException("Unable to read connection string"));
    options.UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());
    options.UseApplicationServiceProvider(sp);
});

builder.Services.AddScoped<WorkerService>();

var app = builder.Build();

app.MapGet("/enqueue_static", async (
    [FromQuery] int count,
    [FromServices] DefaultBackynetContext backynetContext,
    [FromServices] ILogger<Program> logger) =>
{
    for (var i = 1; i <= count; i++)
    {
        var a = Random.Shared.Next(1000, 9999);
        var b = Random.Shared.Next(1000, 9999);

        var jobId = await backynetContext.Client.EnqueueAsync(() => Calculator.Calculate(a, b));
        logger.LogInformation("Job (jobId = {id}) was enqueued", jobId);
    }
});

app.MapGet("/enqueue_instance", async (
    [FromQuery] int count,
    [FromServices] DefaultBackynetContext backynetContext,
    [FromServices] WorkerService workerService,
    [FromServices] ILogger<Program> logger) =>
{
    for (var i = 1; i <= count; i++)
    {
        var jobId = await backynetContext.Client.EnqueueAsync(() => workerService.Execute());
        logger.LogInformation("Job (jobId = {id}) was enqueued", jobId);
    }
});

app.Run();

namespace WebApplication2
{
    /* static class */
    public static class Calculator
    {
        public static async Task Calculate(int a, int b)
        {
            Console.WriteLine($"Executing background job with args {a} {b}");
            await Task.Delay(1000);
            Console.WriteLine("Job executed");
        }
    }

    /* class added to ioc */
    public class WorkerService
    {
        private readonly ILogger<WorkerService> _logger;

        public WorkerService(ILogger<WorkerService> logger)
        {
            _logger = logger;
        }

        public async Task Execute()
        {
            _logger.LogInformation("Executing background job");
            await Task.Delay(1000);
            _logger.LogInformation("Job executed");
        }
    }

    public class DefaultBackynetContext : BackynetContext
    {
        public DefaultBackynetContext(BackynetContextOptions options) : base(options)
        {
        }
    }
}