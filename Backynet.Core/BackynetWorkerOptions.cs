namespace Backynet.Core;

internal sealed class BackynetWorkerOptions
{
    private int _maxThreads;

    public int MaxThreads
    {
        get => _maxThreads;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            _maxThreads = value;
        }
    }

    private string _serverName;

    public string ServerName
    {
        get => _serverName;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            _serverName = value;
        }
    }

    private TimeSpan _poolingInterval;

    public TimeSpan PoolingInterval
    {
        get => _poolingInterval;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, TimeSpan.Zero);
            _poolingInterval = value;
        }
    }

    public BackynetWorkerOptions()
    {
        MaxThreads = Environment.ProcessorCount;
        ServerName = Environment.MachineName;
    }
}