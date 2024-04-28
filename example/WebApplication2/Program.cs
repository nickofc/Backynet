using Backynet.Core.Abstraction;

var builder = WebApplication.CreateBuilder(args);

/* add just backynet client */
builder.Services.AddBackynetClient();

/* add just backynet server */
builder.Services.AddBackynetServer(options =>
{
    options.UseMaximumConcurrencyThreads(10);
    options.UseServerName("fancy-server-name");
    
    options.UsePostgreSql(options =>
    {
        options.UseConnectionString(Environment.GetEnvironmentVariable("BACKYNET_CONNECTION_STRING"));
    });
});

var app = builder.Build();

app.MapGet("/enqueue", async context =>
{
    var backynetClient = context.RequestServices.GetRequiredService<IBackynetClient>();
    await backynetClient.EnqueueAsync(() => Func.DoWork());

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