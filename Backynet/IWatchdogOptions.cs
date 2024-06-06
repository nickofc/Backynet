namespace Backynet;

public interface IWatchdogOptions
{
    TimeSpan PoolingInterval { get; }
    string ServerName { get; }
}