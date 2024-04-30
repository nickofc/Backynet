using Backynet.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBackynetContext<DefaultBackynetContext>(options =>
{
    options.UsePostgreSql(Environment.GetEnvironmentVariable("BACKYNET_CONNECTION_STRING"));
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

public class DefaultBackynetContext : BackynetContext
{
    public DefaultBackynetContext(BackynetContextOptions options) : base(options)
    {
    }
}