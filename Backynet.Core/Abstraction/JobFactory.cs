using System.Linq.Expressions;

namespace Backynet.Core.Abstraction;

public static class JobFactory
{
    public static Job Create(Expression<Func<Task>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var methodMetadata = JobDescriptorFactory.Create(expression);
        return Create(methodMetadata);
    }

    public static Job Create(Expression<Action> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var methodMetadata = JobDescriptorFactory.Create(expression);
        return Create(methodMetadata);
    }

    public static Job Create(IJobDescriptor jobDescriptor)
    {
        ArgumentNullException.ThrowIfNull(jobDescriptor);

        var job = new Job
        {
            Id = Guid.NewGuid(),
            JobState = JobState.Created,
            CreatedAt = DateTimeOffset.UtcNow,
            Descriptor = jobDescriptor
        };

        return job;
    }

    public static Job Create(Expression expression)
    {
        return expression switch
        {
            Expression<Action> x => Create(x),
            Expression<Func<Task>> x => Create(x),
            _ => throw new InvalidOperationException()
        };
    }
}