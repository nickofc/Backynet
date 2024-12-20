namespace Backynet.Abstraction;

public class Job
{
    public static Job Empty() => JobFactory.Create(JobDescriptor.Empty());

    public Guid Id { get; set; }
    public JobState JobState { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public IJobDescriptor Descriptor { get; set; } = default!;
    public string? Cron { get; set; }
    public int RowVersion { get; set; }
    public List<string> Errors { get; set; } = [];
    public Dictionary<string, string> Context { get; set; } = new();
    public Guid? LockId { get; set; }
    public DateTimeOffset? LockExpiresAt { get; set; }
    public DateTimeOffset? NextOccurrenceAt { get; set; }
}