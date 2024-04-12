using Backynet.Core.Abstraction;
using Microsoft.Extensions.Hosting;

namespace Backynet.AspNetCore;

internal sealed class BackynetServerHostedService : BackgroundService
{
    private readonly IBackynetWorker _backynetWorker;

    public BackynetServerHostedService(IBackynetWorker backynetWorker)
    {
        _backynetWorker = backynetWorker;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return _backynetWorker.Start(cancellationToken);
    }
}