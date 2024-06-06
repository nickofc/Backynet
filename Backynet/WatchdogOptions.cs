using Backynet.Options;

namespace Backynet;

public class WatchdogOptions : IWatchdogOptions
{
    public TimeSpan PoolingInterval { get; init; }
    public string ServerName { get; init; } = null!;

    public WatchdogOptions()
    {
    }

    public WatchdogOptions(IBackynetContextOptions backynetContextOptions)
    {
        var coreOptionsExtension = backynetContextOptions.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension();

        PoolingInterval = TimeSpan.FromSeconds(1); // todo: replace
        ServerName = coreOptionsExtension.ServerName;
    }
}