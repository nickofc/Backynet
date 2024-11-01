using Backynet.Abstraction;

namespace Backynet;

public class SystemClock : ISystemClock
{
    public static readonly ISystemClock Instance = new SystemClock();

    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}