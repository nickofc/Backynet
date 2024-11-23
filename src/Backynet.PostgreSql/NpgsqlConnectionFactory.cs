using Npgsql;

namespace Backynet.PostgreSql;

internal sealed class NpgsqlConnectionFactory
{
    private readonly PostgreSqlOptions _postgreSqlOptions;

    public NpgsqlConnectionFactory(PostgreSqlOptions postgreSqlOptions)
    {
        _postgreSqlOptions = postgreSqlOptions;
    }

    public NpgsqlConnection Get(Action<NpgsqlConnectionStringBuilder>? configure = null)
    {
        var connectionString = _postgreSqlOptions.ConnectionString;
        
        if (configure != null)
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            configure.Invoke(connectionStringBuilder);

            connectionString = connectionStringBuilder.ToString();
        }

        return new NpgsqlConnection(connectionString);
    }
}