using Backynet.Abstraction;
using Npgsql;

namespace Backynet.PostgreSql;

internal sealed class PostgreSqlGroupRepository : IGroupRepository
{
    private readonly NpgsqlConnectionFactory _npgsqlConnectionFactory;

    public PostgreSqlGroupRepository(NpgsqlConnectionFactory npgsqlConnectionFactory)
    {
        _npgsqlConnectionFactory = npgsqlConnectionFactory;
    }

    public async Task Add(Group group, CancellationToken cancellationToken = default)
    {
        await using var connection = _npgsqlConnectionFactory.Get();
        await using var command = connection.CreateCommand();
        command.Connection = connection;
        command.CommandText = """
                              insert into groups (group_name, max_concurrent_threads)
                              values (@group_name, @max_concurrent_threads);
                              """;
        command.Parameters.Add(new NpgsqlParameter("group_name", group.GroupName));
        command.Parameters.Add(new NpgsqlParameter<int>("max_concurrent_threads", group.MaxConcurrentThreads));
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task<Group?> Get(string groupName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task Update(string groupName, Group group, CancellationToken cancellationToken = default)
    {
        await using var connection = _npgsqlConnectionFactory.Get();
        await using var command = connection.CreateCommand();
        command.Connection = connection;
        command.CommandText = """
                              update groups 
                              set 
                                  max_concurrent_threads = @max_concurrent_threads
                              where group_name = @group_name
                              """;
        command.Parameters.Add(new NpgsqlParameter("group_name", groupName));
        command.Parameters.Add(new NpgsqlParameter<int>("max_concurrent_threads", group.MaxConcurrentThreads));
        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}