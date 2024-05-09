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

public class Context
{
    public State State { get; }
    public Job Job { get; }
}

public abstract class State
{
    protected Context Context { get; private set; }

    public void SetContext(Context context)
    {
        Context = context;
    }

    public abstract ValueTask Perform();
}

// czy warto iść w state? Moze po prostu jedna klasa? 

public class FailedState : State
{
    public override ValueTask Perform()
    {
        return ValueTask.CompletedTask;
    }
}

public class CreatedState : State
{
    private readonly ICron _cron;
    private readonly ISystemClock _systemClock;
    private readonly IJobRepository _jobRepository;

    public CreatedState(ICron cron, IJobRepository jobRepository, ISystemClock systemClock)
    {
        _cron = cron;
        _jobRepository = jobRepository;
        _systemClock = systemClock;
    }

    public override async ValueTask Perform()
    {
        var job = Context.Job;
        DateTimeOffset? jobNextOccurrence = null;

        if (job.NextOccurrenceAt != null)
        {
            jobNextOccurrence = job.NextOccurrenceAt;
        }
        else if (job.Cron != null)
        {
            jobNextOccurrence = _cron.GetNextOccurrence(job.Cron, _systemClock.UtcNow);
        }

        job.NextOccurrenceAt = jobNextOccurrence;
        job.JobState = JobState.Scheduled;

        await _jobRepository.Update(job.Id, job);
    }
}

public class ScheduledState : State
{
    private readonly ISystemClock _systemClock;
    private readonly IJobExecutor _jobExecutor;

    public ScheduledState(ISystemClock systemClock, IJobExecutor jobExecutor)
    {
        _systemClock = systemClock;
        _jobExecutor = jobExecutor;
    }

    public override async ValueTask Perform()
    {
        var job = Context.Job;

        if (job.NextOccurrenceAt == null)
        {
            throw new InvalidOperationException();
        }

        if (job.NextOccurrenceAt > _systemClock.UtcNow)
        {
            return;
        }
    }
}

public class ProcessingState : State
{
    private readonly IJobExecutor _jobExecutor;

    public ProcessingState(IJobExecutor jobExecutor)
    {
        _jobExecutor = jobExecutor;
    }

    public override async ValueTask Perform()
    {
        await _jobExecutor.Execute(Context.Job);
    }
}