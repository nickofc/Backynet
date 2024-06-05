namespace Backynet;

public class WatchdogOptions
{
    public TimeSpan PoolingInterval { get; set; } = TimeSpan.FromSeconds(1);
    public string ServerName { get; set; }
}