namespace Backynet.Core.Abstraction;

public class Job
{
    public Guid Id { get; set; }
    public JobState JobState { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public IJobDescriptor Descriptor { get; set; }
    public string? ServerName { get; set; }
    public string? Cron { get; set; }
    public string? GroupName { get; set; }
    public DateTimeOffset? NextOccurrenceAt { get; set; }
}