using Backynet.Core.Abstraction;

namespace Backynet.Core;

public static class BackynetServerFactory
{
    public static IBackynetServer Create(BackynetServerOptions backynetServerOptions)
    {
        //todo: jak przekazac to z innej paczki?
        IJobRepository jobRepository = null;
        IJobExecutor jobExecutor = null;
        IServerService serverService = null;
        IThreadPool threadPool = null;

        return new BackynetServer(jobRepository, jobExecutor, backynetServerOptions, serverService, threadPool);
    }
}