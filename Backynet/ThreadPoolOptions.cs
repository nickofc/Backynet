using Backynet.Options;

namespace Backynet;

public class ThreadPoolOptions : IThreadPoolOptions
{
    public int MaxThreads { get; init; }

    public ThreadPoolOptions()
    {
    }

    public ThreadPoolOptions(IBackynetContextOptions backynetContextOptions)
    {
        var coreOptionsExtension = backynetContextOptions.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension();

        MaxThreads = coreOptionsExtension.MaxThreads;
    }
}