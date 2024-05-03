using Npgsql;

namespace Backynet.PostgreSql;

internal sealed class NpgsqlConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<NpgsqlConnection> GetAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new NpgsqlConnection(_connectionString));
    }
}