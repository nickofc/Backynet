namespace Backynet.Abstraction;

public interface IJobDescriptorExecutor
{
    Task Execute(JobDescriptor jobDescriptor, CancellationToken cancellationToken = default);
}