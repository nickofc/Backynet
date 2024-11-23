namespace Backynet.PostgreSql;

internal sealed class MigrationService
{
    private readonly NpgsqlConnectionFactory _connectionFactory;

    public MigrationService(NpgsqlConnectionFactory connectionFactory, PostgreSqlOptions postgreSqlOptions)
    {
        _connectionFactory = connectionFactory;
        _createInfrastructureSql = string.Format(Sql.CreateInfrastructureSql, postgreSqlOptions.Schema);
        _dropInfrastructureSql = string.Format(Sql.DropInfrastructureSql, postgreSqlOptions.Schema);
    }

    private readonly string _createInfrastructureSql;
    private readonly string _dropInfrastructureSql;

    public async Task Up(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Get();
        await using var command = connection.CreateCommand();
        command.CommandText = _createInfrastructureSql;
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task Down(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.Get();
        await using var command = connection.CreateCommand();
        command.CommandText = _dropInfrastructureSql;
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static class Sql
    {
        public const string CreateInfrastructureSql =
            """
            CREATE SCHEMA IF NOT EXISTS {0};

            CREATE TABLE IF NOT EXISTS {0}.jobs
            (
                id              uuid                     NOT NULL
                    CONSTRAINT jobs_pk PRIMARY KEY,
                descriptor      bytea,
                context         bytea,
                errors          bytea,
                cron            text,
                state           integer                           DEFAULT 0,
                created_at      timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                row_version     integer                           DEFAULT 0,
                lock_id         uuid,
                lock_expires_at timestamp with time zone,
                next_occurrence_at timestamp with time zone
            );
            """;

        public const string DropInfrastructureSql = "DROP SCHEMA IF EXISTS {0} CASCADE";
    }
}