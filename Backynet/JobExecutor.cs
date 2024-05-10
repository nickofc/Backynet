using Backynet.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace Backynet;

public class JobExecutor : IJobExecutor
{
    private readonly IJobDescriptorExecutor _jobDescriptorExecutor;
    private readonly IJobRepository _jobRepository;

    public JobExecutor(IJobDescriptorExecutor jobDescriptorExecutor, IJobRepository jobRepository)
    {
        _jobDescriptorExecutor = jobDescriptorExecutor;
        _jobRepository = jobRepository;
    }

    // todo: state machine for jobs?
    public async Task Execute(Job job, CancellationToken cancellationToken = default)
    {
        try
        {
            await _jobDescriptorExecutor.Execute(job.Descriptor, cancellationToken);

            job.JobState = JobState.Succeeded;
            await _jobRepository.Update(job.Id, job, cancellationToken);
        }
        catch (Exception)
        {
            job.JobState = JobState.Failed;
            await _jobRepository.Update(job.Id, job, cancellationToken);
        }
    }
}

public class JobStateFactory
{
    private readonly IServiceProvider _serviceProvider;

    public JobStateFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private static readonly Dictionary<JobState, Type> InternalStateMap = new()
    {
        { JobState.Created, typeof(CreatedState) },
        { JobState.Scheduled, typeof(ScheduledState) }
    };

    public bool TryCreate(JobState jobState, out IState? state)
    {
        if (!InternalStateMap.TryGetValue(jobState, out var type))
        {
            state = null;
            return false;
        }

        var service = _serviceProvider.GetRequiredService(type);
        state = service as IState;

        return true;
    }
}

public interface IState
{
    void SetContext();
    ValueTask Execute(CancellationToken cancellationToken);
}

public class CreatedState : IState
{
    public void SetContext()
    {
        throw new NotImplementedException();
    }

    public ValueTask Execute(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class ScheduledState : IState
{
    public void SetContext()
    {
        throw new NotImplementedException();
    }

    public ValueTask Execute(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

// public class FailedState : IState<JobState>
// {
//     public void SetContext()
//     {
//         throw new NotImplementedException();
//     }
//
//     ValueTask<JobState> IState<JobState>.Execute(CancellationToken cancellationToken)
//     {
//         return new ValueTask<JobState>(JobState.Created);
//     }
// }
//
// public class CreatedState : IState
// {
//     private readonly ICron _cron;
//     private readonly ISystemClock _systemClock;
//     private readonly IJobRepository _jobRepository;
//
//     public CreatedState(ICron cron, IJobRepository jobRepository, ISystemClock systemClock)
//     {
//         _cron = cron;
//         _jobRepository = jobRepository;
//         _systemClock = systemClock;
//     }
//
//     public override async ValueTask Execute()
//     {
//         var job = Context.Job;
//         DateTimeOffset? jobNextOccurrence = null;
//
//         if (job.NextOccurrenceAt != null)
//         {
//             jobNextOccurrence = job.NextOccurrenceAt;
//         }
//         else if (job.Cron != null)
//         {
//             jobNextOccurrence = _cron.GetNextOccurrence(job.Cron, _systemClock.UtcNow);
//         }
//
//         job.NextOccurrenceAt = jobNextOccurrence;
//         job.JobState = JobState.Scheduled;
//
//         await _jobRepository.Update(job.Id, job);
//     }
//
//     public ValueTask Execute(CancellationToken cancellationToken)
//     {
//         throw new NotImplementedException();
//     }
// }
//
// public class ScheduledState : State
// {
//     private readonly ISystemClock _systemClock;
//     private readonly IJobExecutor _jobExecutor;
//
//     public ScheduledState(ISystemClock systemClock, IJobExecutor jobExecutor)
//     {
//         _systemClock = systemClock;
//         _jobExecutor = jobExecutor;
//     }
//
//     public override async ValueTask Perform()
//     {
//         var job = Context.Job;
//
//         if (job.NextOccurrenceAt == null)
//         {
//             throw new InvalidOperationException();
//         }
//
//         if (job.NextOccurrenceAt > _systemClock.UtcNow)
//         {
//             return;
//         }
//     }
// }
//
// public class ProcessingState : State
// {
//     private readonly IJobExecutor _jobExecutor;
//
//     public ProcessingState(IJobExecutor jobExecutor)
//     {
//         _jobExecutor = jobExecutor;
//     }
//
//     public override async ValueTask Perform()
//     {
//         await _jobExecutor.Execute(Context.Job);
//     }
// }