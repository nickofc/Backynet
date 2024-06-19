using Backynet.Options;

namespace Backynet;

internal sealed class BackynetServerOptions : IBackynetServerOptions
{
    public Guid InstanceId { get; init; } 
    public string ServerName { get; init; } = null!;
    public TimeSpan HeartbeatInterval { get; init; }
    public TimeSpan PoolingInterval { get; init; }

    public BackynetServerOptions()
    {
    }

    public BackynetServerOptions(IBackynetContextOptions backynetContextOptions)
    {
        var coreOptionsExtension = backynetContextOptions.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension();

        ServerName = coreOptionsExtension.ServerName;
        HeartbeatInterval = coreOptionsExtension.HeartbeatInterval;
        PoolingInterval = coreOptionsExtension.PoolingInterval;
        InstanceId = coreOptionsExtension.InstanceId;
    }
}