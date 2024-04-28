using Backynet.Core;
using Backynet.Core.Abstraction;
using Backynet.Postgresql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Backynet.PostgreSql;

public class PostgreSqlOptionsExtension : IBackynetContextOptionsExtension
{
    private string? _connectionString;

    public PostgreSqlOptionsExtension()
    {
    }

    protected PostgreSqlOptionsExtension(PostgreSqlOptionsExtension copyFrom)
    {
        _connectionString = copyFrom._connectionString;
    }

    public virtual void ApplyServices(IServiceCollection services)
    {
        services.TryAddSingleton<IJobRepository, PostgreSqlJobRepository>();
        services.TryAddSingleton<IServerService, ServerService>();
    }

    public virtual PostgreSqlOptionsExtension WithConnectionString(string connectionString)
    {
        var clone = Clone();

        clone._connectionString = connectionString;

        return clone;
    }

    protected virtual PostgreSqlOptionsExtension Clone()
    {
        return new PostgreSqlOptionsExtension(this);
    }
}