namespace Backynet.Core;

public class BackynetHostOptions
{
    public TimeSpan HeartbeatInterval { get; set; }
    public string ServerName { get; set; }
}