using Microsoft.Extensions.Logging;

namespace Backynet;

public static partial class Log
{

    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Context for `{ContextType}` was created")]
    public static partial void ContextCreated(this ILogger logger, string contextType);
}