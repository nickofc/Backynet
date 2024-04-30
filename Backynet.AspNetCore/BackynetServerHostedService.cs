using Backynet.Core.Abstraction;
using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace Backynet.Core;

internal sealed class BackynetServerHostedService : BackgroundService
{
    private readonly IBackynetServer _backynetServer;

    public BackynetServerHostedService(IBackynetServer backynetServer)
    {
        _backynetServer = backynetServer;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        return _backynetServer.Start(cancellationToken);
    }
}