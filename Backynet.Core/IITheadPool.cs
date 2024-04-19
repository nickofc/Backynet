namespace Backynet.Core;

public interface IITheadPool
{
    Task WaitToPostAsync(CancellationToken cancellationToken = default);
    Task PostAsync(Func<Task> func);
}