using System.Linq.Expressions;

namespace Backynet.Abstraction;

public interface IBackynetClient
{
    Task<Guid> EnqueueAsync(Expression<Func<Task>> expression, CancellationToken cancellationToken = default) =>
        EnqueueAsync(expression, when: null, cancellationToken: cancellationToken);

    Task<Guid> EnqueueAsync(Expression<Action> expression, CancellationToken cancellationToken = default) =>
        EnqueueAsync(expression, when: null, cancellationToken: cancellationToken);

    Task<Guid> EnqueueAsync(Expression expression, DateTimeOffset? when = null,
        string? cron = null, CancellationToken cancellationToken = default);

    Task<bool> CancelAsync(Guid jobId, CancellationToken cancellationToken = default);
}