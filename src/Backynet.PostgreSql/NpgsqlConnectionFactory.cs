using Npgsql;

namespace Backynet.PostgreSql;

internal sealed class NpgsqlConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public NpgsqlConnection Get(Action<NpgsqlConnectionStringBuilder>? configure = null)
    {
        var connectionString = _connectionString;
        
        if (configure != null)
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(_connectionString);
            configure.Invoke(connectionStringBuilder);

            connectionString = connectionStringBuilder.ToString();
        }

        return new NpgsqlConnection(connectionString);
    }
}