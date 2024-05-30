using Npgsql;

namespace Backynet.PostgreSql;

internal sealed class NpgsqlConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public NpgsqlConnection Get()
    {
        return new NpgsqlConnection(_connectionString);
    }
}