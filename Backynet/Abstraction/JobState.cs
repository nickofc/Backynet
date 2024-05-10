namespace Backynet.Abstraction;

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