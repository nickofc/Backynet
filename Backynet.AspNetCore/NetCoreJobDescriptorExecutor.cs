using Backynet.Core.Abstraction;

namespace Backynet.AspNetCore;

public class NetCoreJobDescriptorExecutor : IJobDescriptorExecutor
{
    // TODO: IOC
    public Task Execute(IJobDescriptor jobDescriptor, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}