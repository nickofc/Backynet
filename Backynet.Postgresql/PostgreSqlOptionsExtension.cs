using Backynet.Abstraction;
using Backynet.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Backynet.PostgreSql;

public class PostgreSqlOptionsExtension : IBackynetContextOptionsExtension
{
    private string _connectionString;
    private TimeSpan? _commandTimeout;
    private bool? _isAutomaticMigrationEnabled;

    public PostgreSqlOptionsExtension()
    {
    }

    protected PostgreSqlOptionsExtension(PostgreSqlOptionsExtension copyFrom)
    {
        _connectionString = copyFrom._connectionString;
        _commandTimeout = copyFrom._commandTimeout;
        _isAutomaticMigrationEnabled = copyFrom._isAutomaticMigrationEnabled;
    }

    public virtual void ApplyServices(IServiceCollection services)
    {
        services.TryAddSingleton(new NpgsqlConnectionFactory(_connectionString));
        services.TryAddSingleton<IPostgreSqlJobRepositoryOptions, PostgreSqlJobRepositoryOptions>();
        services.TryAddSingleton<IJobRepository, PostgreSqlJobRepository>();
        services.TryAddSingleton<IServerServiceOptions, ServerServiceOptions>();
        services.TryAddSingleton<IServerService, ServerService>();
    }

    public void Validate(IBackynetContextOptions options)
    {
    }

    public virtual string ConnectionString => _connectionString;

    public virtual PostgreSqlOptionsExtension WithConnectionString(string connectionString)
    {
        var clone = Clone();

        clone._connectionString = connectionString;

        return clone;
    }

    public virtual TimeSpan? CommandTimeout => _commandTimeout;

    public virtual PostgreSqlOptionsExtension WithCommandTimeout(TimeSpan? commandTimeout)
    {
        var clone = Clone();

        clone._commandTimeout = commandTimeout;

        return clone;
    }

    public virtual bool? IsAutomaticMigrationEnabled => _isAutomaticMigrationEnabled;

    public virtual PostgreSqlOptionsExtension WithAutomaticMigration(bool? isAutomaticMigrationEnabled)
    {
        var clone = Clone();

        clone._isAutomaticMigrationEnabled = isAutomaticMigrationEnabled;

        return clone;
    }

    protected virtual PostgreSqlOptionsExtension Clone()
    {
        return new PostgreSqlOptionsExtension(this);
    }
}