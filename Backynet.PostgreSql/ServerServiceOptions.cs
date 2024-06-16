using Backynet.Options;

namespace Backynet.PostgreSql;

internal sealed class ServerServiceOptions : IServerServiceOptions
{
    public TimeSpan MaxTimeWithoutHeartbeat { get; init; }
    public Guid InstanceId { get; init; }
    public string ServerName { get; init; }

    public ServerServiceOptions()
    {
    }
    
    public ServerServiceOptions(IBackynetContextOptions backynetContextOptions)
    {
        var coreOptionsExtension = backynetContextOptions.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension();

        MaxTimeWithoutHeartbeat = coreOptionsExtension.MaxTimeWithoutHeartbeat;
        ServerName = coreOptionsExtension.ServerName;

        InstanceId = Guid.NewGuid();
    }
}