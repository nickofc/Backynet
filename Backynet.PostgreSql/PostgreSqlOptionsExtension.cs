using Backynet.Abstraction;
using Backynet.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Backynet.PostgreSql;

public class PostgreSqlOptionsExtension : IBackynetContextOptionsExtension
{
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
        services.TryAddSingleton<IWatchdogRepository, WatchdogRepository>();
    }

    public void Validate(IBackynetContextOptions options)
    {
    }

    private string _connectionString;
    public virtual string ConnectionString => _connectionString;

    public virtual PostgreSqlOptionsExtension WithConnectionString(string connectionString)
    {
        var clone = Clone();

        clone._connectionString = connectionString;

        return clone;
    }

    private TimeSpan? _commandTimeout;
    public virtual TimeSpan? CommandTimeout => _commandTimeout;

    public virtual PostgreSqlOptionsExtension WithCommandTimeout(TimeSpan? commandTimeout)
    {
        var clone = Clone();

        clone._commandTimeout = commandTimeout;

        return clone;
    }

    private bool? _isAutomaticMigrationEnabled;
    public virtual bool? IsAutomaticMigrationEnabled => _isAutomaticMigrationEnabled;

    public virtual PostgreSqlOptionsExtension WithAutomaticMigration(bool? isAutomaticMigrationEnabled)
    {
        var clone = Clone();

        clone._isAutomaticMigrationEnabled = isAutomaticMigrationEnabled;

        return clone;
    }

    private string? _schema;
    public virtual string? Schema => _schema;

    public PostgreSqlOptionsExtension WithSchema(string? schema)
    {
        var clone = Clone();

        clone._schema = schema;

        return clone;
    }

    protected virtual PostgreSqlOptionsExtension Clone()
    {
        return new PostgreSqlOptionsExtension(this);
    }
}