using System.Linq.Expressions;

namespace Backynet.Abstraction;

public interface IBackynetClient
{
    Task<Guid> EnqueueAsync(Expression<Func<Task>> expression, CancellationToken cancellationToken = default) =>
        EnqueueAsync(expression, groupName: null, when: null, cancellationToken: cancellationToken);

    Task<Guid> EnqueueAsync(Expression<Action> expression, CancellationToken cancellationToken = default) =>
        EnqueueAsync(expression, groupName: null, when: null, cancellationToken: cancellationToken);

    Task<Guid> EnqueueAsync(Expression expression, string? groupName = null, DateTimeOffset? when = null,
        string? cron = null, CancellationToken cancellationToken = default);

    Task<bool> CancelAsync(Guid jobId, CancellationToken cancellationToken = default);
}