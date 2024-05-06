using Backynet;
using Backynet.Options;
using WebApplication2;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();
builder.Services.AddBackynetContext<DefaultBackynetContext>((sp, options) =>
{
    options.UsePostgreSql(Environment.GetEnvironmentVariable("BACKYNET_CONNECTION_STRING") ??
                          throw new InvalidOperationException("Unable to read connection string"));
    options.UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());
    options.UseApplicationServiceProvider(sp);
});

builder.Services.AddScoped<WorkerService>();

var app = builder.Build();

app.MapGet("/enqueue_static", async (
    [FromServices] DefaultBackynetContext backynetContext,
    [FromServices] ILogger<Program> logger) =>
{
    var jobId = await backynetContext.Client.EnqueueAsync(() => Func.DoWork());
    logger.LogInformation("Job (jobId = {id}) was enqueued", jobId);
});

app.MapGet("/enqueue_class_instance", async (
    [FromServices] DefaultBackynetContext backynetContext,
    [FromServices] WorkerService workerService,
    [FromServices] ILogger<Program> logger) =>
{
    var jobId = await backynetContext.Client.EnqueueAsync(() => workerService.Execute());
    logger.LogInformation("Job (jobId = {id}) was enqueued", jobId);
});

app.MapGet("/enqueue_class_instance_bulk", async (
    [FromServices] DefaultBackynetContext backynetContext,
    [FromServices] WorkerService workerService,
    [FromServices] ILogger<Program> logger) =>
{
    for (var i = 0; i < 1000; i++)
    {
        var jobId = await backynetContext.Client.EnqueueAsync(() => workerService.Execute());
        logger.LogInformation("Job (jobId = {id}) was enqueued", jobId);
    }
});

app.Run();

namespace WebApplication2
{
    /* static class */
    public static class Func
    {
        public static async Task DoWork()
        {
            Console.WriteLine("Executing job");
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
        public DefaultBackynetContext(BackynetContextOptions<DefaultBackynetContext> options) : base(options)
        {
        }
    }
}