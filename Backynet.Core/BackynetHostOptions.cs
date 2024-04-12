namespace Backynet.Core;

public class BackynetHostOptions
{
    public TimeSpan HostTimeout { get; set; }
    public TimeSpan HeartbeatInterval { get; set; }
    public string ServerName { get; set; }
}