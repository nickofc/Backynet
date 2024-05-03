using Backynet.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBackynetContext<DefaultBackynetContext>((sp, options) =>
{
    options.UseHeartbeatInterval(TimeSpan.FromSeconds(30));
    options.UsePoolingInterval(TimeSpan.FromSeconds(1));
    options.UseServerName(Environment.MachineName);
    options.UseMaxTimeWithoutHeartbeat(TimeSpan.FromSeconds(120));
    options.UseMaxThreads(20);
    options.UsePostgreSql(Environment.GetEnvironmentVariable("BACKYNET_CONNECTION_STRING"));
    options.UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());
});

var app = builder.Build();

app.MapGet("/enqueue", async context =>
{
    var backynetClient = context.RequestServices.GetRequiredService<DefaultBackynetContext>();
    await backynetClient.Client.EnqueueAsync(() => Func.DoWork());

    Console.WriteLine("Job was enqueued");
});

app.Run();


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

public class DefaultBackynetContext : BackynetContext;