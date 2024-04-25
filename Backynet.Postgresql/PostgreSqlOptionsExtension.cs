using Backynet.Core;
using Backynet.Core.Abstraction;
using Backynet.Postgresql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Backynet.PostgreSql;

internal sealed class PostgreSqlOptionsExtension : IBackynetContextOptionsExtension
{
    public void ApplyServices(IServiceCollection services)
    {
        services.TryAddSingleton<IJobRepository, PostgreSqlJobRepository>();
        services.TryAddSingleton<IServerService, ServerService>();
    }
}