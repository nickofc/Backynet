using Backynet.Abstraction;
using Microsoft.Extensions.Hosting;

namespace Backynet.PostgreSql;

internal sealed class PostgreSqlBackynetServer : IBackynetServer, IHostedService
{
    private readonly PostgreSqlWorker _postgreSqlWorker;

    public PostgreSqlBackynetServer(PostgreSqlWorker postgreSqlWorker)
    {
        _postgreSqlWorker = postgreSqlWorker;
    }

    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _startTask;

    public Task Start(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _startTask = _postgreSqlWorker.Start(_cancellationTokenSource.Token);

        if (_startTask.IsCompleted)
        {
            return _startTask;
        }

        return Task.CompletedTask;
    }

    public async Task Stop(CancellationToken cancellationToken)
    {
        if (_startTask == null)
        {
            return;
        }

        try
        {
            await _cancellationTokenSource!.CancelAsync();
        }
        finally
        {
            var tcs = new TaskCompletionSource();
            await using var registration = cancellationToken.Register(s => ((TaskCompletionSource<object>)s!).SetCanceled(cancellationToken), tcs);
            await Task.WhenAny(_startTask, tcs.Task);
        }
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        return Start(cancellationToken);
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        return Stop(cancellationToken);
    }
}