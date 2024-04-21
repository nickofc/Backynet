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

public class Context
{
    public State State { get; }
}

public abstract class State
{
    protected Context Context { get; private set; }

    public void SetContext(Context context)
    {
        Context = context;
    }

    public abstract void Handle();
}

public class FailedState : State
{
    public override void Handle()
    {
    }
}

public class CreatedState : State
{
    public override void Handle()
    {
    }
}