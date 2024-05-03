using Backynet.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Backynet;

public static class ServiceProviderCache
{
    public static IServiceProvider Get(IBackynetContextOptions options)
    {
        var services = new ServiceCollection();

        ValidateOptions(options);
        ApplyServices(options, services);

        return services.BuildServiceProvider();
    }

    private static void ApplyServices(IBackynetContextOptions options, ServiceCollection services)
    {
        foreach (var extension in options.Extensions)
        {
            extension.ApplyServices(services);
        }

        new BackynetServicesBuilder(services).TryAddCoreServices();
    }

    private static void ValidateOptions(IBackynetContextOptions options)
    {
        foreach (var extension in options.Extensions)
        {
            extension.Validate(options);
        }
    }
}