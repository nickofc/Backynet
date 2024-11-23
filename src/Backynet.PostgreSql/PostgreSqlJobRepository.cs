using Backynet.Abstraction;
using Backynet.PostgreSql.Internal;
using Npgsql;

namespace Backynet.PostgreSql;

internal sealed class PostgreSqlJobRepository : IJobRepository
{
    private readonly NpgsqlConnectionFactory _npgsqlConnectionFactory;
    private readonly ISerializer _serializer;
    private readonly PostgreSqlOptions _postgreSqlOptions;

    public PostgreSqlJobRepository(
        NpgsqlConnectionFactory npgsqlConnectionFactory,
        ISerializer serializer, PostgreSqlOptions postgreSqlOptions)
    {
        _npgsqlConnectionFactory = npgsqlConnectionFactory;
        _serializer = serializer;
        _postgreSqlOptions = postgreSqlOptions;
    }

    public async Task Add(Job job, CancellationToken cancellationToken = default)
    {
        await using var connection = _npgsqlConnectionFactory.Get();
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
                               insert into {_postgreSqlOptions.Schema}.jobs (id, state, created_at, descriptor, cron, next_occurrence_at, row_version, errors, context)
                               values (@id, @state, @created_at, @descriptor, @cron, @next_occurrence_at, @row_version, @errors, @context);
                               """;

        command.Parameters.Add(new NpgsqlParameter<Guid>("id", job.Id));
        command.Parameters.Add(new NpgsqlParameter<int>("state", CastTo<int>.From(job.JobState)));
        command.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("created_at", job.CreatedAt));
        command.Parameters.Add(new NpgsqlParameter<ReadOnlyMemory<byte>>("descriptor", _serializer.Serialize(job.Descriptor)));
        command.Parameters.Add(new NpgsqlParameter<string?>("cron", job.Cron));
        command.Parameters.Add(new NpgsqlParameter<DateTimeOffset?>("next_occurrence_at", job.NextOccurrenceAt));
        command.Parameters.Add(new NpgsqlParameter<int>("row_version", job.RowVersion));
        command.Parameters.Add(new NpgsqlParameter<ReadOnlyMemory<byte>>("errors", _serializer.Serialize(job.Errors)));
        command.Parameters.Add(new NpgsqlParameter<ReadOnlyMemory<byte>>("context", _serializer.Serialize(job.Context)));

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Job?> Get(Guid jobId, CancellationToken cancellationToken = default)
    {
        await using var connection = _npgsqlConnectionFactory.Get();
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
                              select id, state, created_at, descriptor, cron, next_occurrence_at, row_version, errors, context
                              from {_postgreSqlOptions.Schema}.jobs where id = @id
                              """;
        command.Parameters.Add(new NpgsqlParameter<Guid>("id", jobId));
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? RowParser.ParseJobRow(_serializer, reader) : null;
    }

    public async Task<bool> Update(Guid jobId, Job job, CancellationToken cancellationToken = default)
    {
        await using var connection = _npgsqlConnectionFactory.Get();
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
                              update {_postgreSqlOptions.Schema}.jobs job
                              set
                                  state = @state,
                                  descriptor = @descriptor,
                                  created_at = @created_at,
                                  cron = @cron,
                                  next_occurrence_at = @next_occurrence_at,
                                  row_version = @row_version + 1,
                                  errors = @errors,
                                  context = @context
                              where job.id = @id and job.row_version = @row_version;
                              """;
        command.Parameters.Add(new NpgsqlParameter<Guid>("id", jobId));
        command.Parameters.Add(new NpgsqlParameter<int>("state", CastTo<int>.From(job.JobState)));
        command.Parameters.Add(new NpgsqlParameter<ReadOnlyMemory<byte>>("descriptor", _serializer.Serialize(job.Descriptor)));
        command.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("created_at", job.CreatedAt));
        command.Parameters.Add(new NpgsqlParameter<string?>("cron", job.Cron));
        command.Parameters.Add(new NpgsqlParameter<DateTimeOffset?>("next_occurrence_at", job.NextOccurrenceAt));
        command.Parameters.Add(new NpgsqlParameter<int>("row_version", job.RowVersion));
        command.Parameters.Add(new NpgsqlParameter<ReadOnlyMemory<byte>>("errors", _serializer.Serialize(job.Errors)));
        command.Parameters.Add(new NpgsqlParameter<ReadOnlyMemory<byte>>("context", _serializer.Serialize(job.Context)));

        await connection.OpenAsync(cancellationToken);
        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

        return rowsAffected > 0;
    }
}