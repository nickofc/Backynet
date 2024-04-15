namespace Backynet.Core.Abstraction;

public enum JobState
{
    Unknown,
    Created,
    Enqueued,
    Scheduled,
    Processing,
    Failed,
    Succeeded,
    Deleted
}