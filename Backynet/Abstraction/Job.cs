namespace Backynet.Abstraction;

public class Job
{
    public static Job Empty() => JobFactory.Create(JobDescriptor.Empty());

    public Guid Id { get; set; }
    public JobState JobState { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public IJobDescriptor Descriptor { get; set; }
    public string? ServerName { get; set; }
    public string? Cron { get; set; }
    public string? GroupName { get; set; }
    public DateTimeOffset? NextOccurrenceAt { get; set; }
    public int RowVersion { get; set; }
    public List<Exception> Errors { get; set; }
    public Dictionary<string, string> Context { get; set; }
}