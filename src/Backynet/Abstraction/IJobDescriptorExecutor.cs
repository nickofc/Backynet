namespace Backynet.Abstraction;

public interface IJobDescriptorExecutor
{
    Task Execute(IJobDescriptor jobDescriptor, CancellationToken cancellationToken = default);
}