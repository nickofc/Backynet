using Backynet.Abstraction;
using Backynet.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Backynet.PostgreSql;

public class PostgreSqlOptionsExtension : IBackynetContextOptionsExtension
{
    private const string DefaultSchemaName = "backynet";
    private static readonly TimeSpan DefaultLockDuration = TimeSpan.FromMinutes(5);
    private const int DefaultFetchSize = 50;

    public PostgreSqlOptionsExtension()
    {
    }

    protected PostgreSqlOptionsExtension(PostgreSqlOptionsExtension copyFrom)
    {
        _connectionString = copyFrom._connectionString;
        _isAutomaticMigrationEnabled = copyFrom._isAutomaticMigrationEnabled;
    }

    public virtual void ApplyServices(IServiceCollection services)
    {
        services.TryAddSingleton<PostgreSqlOptions>();
        services.TryAddSingleton<NpgsqlConnectionFactory>();

        services.TryAddSingleton<IJobRepository, PostgreSqlJobRepository>();
        services.TryAddSingleton<PostgreSqlJobQueue>();

        services.TryAddScoped<MigrationService>();
        services.AddHostedService<MigrationHostedService>();

        services.TryAddScoped<PostgreSqlJobDataProducer>();
        services.TryAddScoped<PostgreSqlJobDataConsumer>();
        services.TryAddScoped<PostgreSqlWorker>();

        services.AddHostedService<PostgreSqlBackynetServer>();
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

    private bool _isAutomaticMigrationEnabled;
    public virtual bool IsAutomaticMigrationEnabled => _isAutomaticMigrationEnabled;

    public virtual PostgreSqlOptionsExtension WithAutomaticMigration(bool isAutomaticMigrationEnabled)
    {
        var clone = Clone();

        clone._isAutomaticMigrationEnabled = isAutomaticMigrationEnabled;

        return clone;
    }

    private string _schema = DefaultSchemaName;
    public virtual string Schema => _schema;

    public PostgreSqlOptionsExtension WithSchema(string schema)
    {
        var clone = Clone();

        clone._schema = schema;

        return clone;
    }

    private bool _dropSchemaOnShutdown;
    public virtual bool DropSchemaOnShutdown => _dropSchemaOnShutdown;

    public PostgreSqlOptionsExtension WithDropSchemaOnShutdown(bool dropSchemaOnShutdown)
    {
        var clone = Clone();

        clone._dropSchemaOnShutdown = dropSchemaOnShutdown;

        return clone;
    }

    private TimeSpan _lockDuration = DefaultLockDuration;
    public virtual TimeSpan LockDuration => _lockDuration;

    public PostgreSqlOptionsExtension WithLockDuration(TimeSpan lockDuration)
    {
        var clone = Clone();

        clone._lockDuration = lockDuration;

        return clone;
    }

    private int _fetchSize = DefaultFetchSize;
    public virtual int FetchSize => _fetchSize;
    
    public PostgreSqlOptionsExtension WithFetchSize(int fetchSize)
    {
        var clone = Clone();

        clone._fetchSize = fetchSize;

        return clone;
    }

    public TimeSpan PoolingInterval { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromSeconds(10);

    protected virtual PostgreSqlOptionsExtension Clone()
    {
        return new PostgreSqlOptionsExtension(this);
    }
}