namespace Backynet.Abstraction;

public static class JobFactory
{
    public static Job Create(IJobDescriptor jobDescriptor)
    {
        ArgumentNullException.ThrowIfNull(jobDescriptor);

        var job = new Job
        {
            Id = Guid.NewGuid(),
            JobState = JobState.Created,
            CreatedAt = DateTimeOffset.UtcNow,
            Descriptor = jobDescriptor,
            NextOccurrenceAt = DateTimeOffset.UtcNow,
            RowVersion = 1
        };

        return job;
    }
}