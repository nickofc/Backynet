using Microsoft.Extensions.DependencyInjection;

namespace Backynet.Options;

public interface IBackynetContextOptionsExtension
{
    void ApplyServices(IServiceCollection services);

    IBackynetContextOptionsExtension ApplyDefaults(IBackynetContextOptions options) => this;

    void Validate(IBackynetContextOptions options);
}