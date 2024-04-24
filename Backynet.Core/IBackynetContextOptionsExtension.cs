using Microsoft.Extensions.DependencyInjection;

namespace Backynet.Core;

public interface IBackynetContextOptionsExtension
{
    void ApplyServices(IServiceCollection services);
}