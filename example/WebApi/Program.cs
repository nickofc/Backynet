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
});

var app = builder.Build();

app.MapGet("/enqueue", async ([FromServices] DefaultBackynetContext backynetContext, [FromServices] ILogger<Program> logger) =>
{
    var jobId = await backynetContext.Client.EnqueueAsync(() => Func.DoWork());
    logger.LogInformation("Job (jobId = {id}) was enqueued", jobId);
});

app.Run();

namespace WebApplication2
{
    /* static class and static method is required.. for now */
    public static class Func
    {
        public static async Task DoWork()
        {
            /* dependency injection is not yet supported :( */
            Console.WriteLine("Executing job");
            await Task.Delay(1000);
            Console.WriteLine("Job executed");
        }
    }

    public class DefaultBackynetContext : BackynetContext
    {
        public DefaultBackynetContext(BackynetContextOptions<DefaultBackynetContext> options) : base(options)
        {
        }
    }
}