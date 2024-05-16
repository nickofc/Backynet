using Microsoft.Extensions.Logging;

namespace Backynet;

public static partial class Log
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Worker starting")]
    public static partial void WorkerStarting(this ILogger logger);
    
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Worker started")]
    public static partial void WorkerStarted(this ILogger logger);
    
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Context was created")]
    public static partial void ContextCreated(this ILogger logger, string contextType);
}