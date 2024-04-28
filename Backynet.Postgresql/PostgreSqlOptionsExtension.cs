using Backynet.Core;
using Backynet.Core.Abstraction;
using Backynet.Postgresql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Backynet.PostgreSql;

public class PostgreSqlOptionsExtension : IBackynetContextOptionsExtension
{
    private string? _connectionString;
    private int? _commandTimeout;

    public PostgreSqlOptionsExtension()
    {
    }

    protected PostgreSqlOptionsExtension(PostgreSqlOptionsExtension copyFrom)
    {
        _connectionString = copyFrom._connectionString;
        _commandTimeout = copyFrom._commandTimeout;
    }

    public virtual void ApplyServices(IServiceCollection services)
    {
        services.TryAddSingleton<IJobRepository, PostgreSqlJobRepository>();
        services.TryAddSingleton<IServerService, ServerService>();
    }

    public void Validate(IBackynetContextOptions options)
    {
    }

    public virtual string? ConnectionString => _connectionString;

    public virtual PostgreSqlOptionsExtension WithConnectionString(string? connectionString)
    {
        var clone = Clone();

        clone._connectionString = connectionString;

        return clone;
    }

    public virtual int? CommandTimeout => _commandTimeout;

    public virtual PostgreSqlOptionsExtension WithCommandTimeout(int? commandTimeout)
    {
        var clone = Clone();

        clone._commandTimeout = commandTimeout;

        return clone;
    }

    protected virtual PostgreSqlOptionsExtension Clone()
    {
        return new PostgreSqlOptionsExtension(this);
    }
}