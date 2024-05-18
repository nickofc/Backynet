using Backynet.Abstraction;
using Npgsql;

namespace Backynet.PostgreSql;

internal class PostgreSqlJobRepository : IJobRepository
{
    private readonly NpgsqlConnectionFactory _npgsqlConnectionFactory;
    private readonly ISerializer _serializer;
    private readonly ISystemClock _systemClock;

    public PostgreSqlJobRepository(
        NpgsqlConnectionFactory npgsqlConnectionFactory,
        ISerializer serializer,
        ISystemClock systemClock)
    {
        _npgsqlConnectionFactory = npgsqlConnectionFactory;
        _serializer = serializer;
        _systemClock = systemClock;
    }

    private static readonly int[] IntermediateStates =
    [
        (int)JobState.Unknown,
        (int)JobState.Created,
        (int)JobState.Enqueued,
        (int)JobState.Scheduled,
        (int)JobState.Processing
    ];

    public async Task<IReadOnlyCollection<Job>> Acquire(string serverName, int maxJobsCount, CancellationToken cancellationToken = default)
    {
        await using var connection = await _npgsqlConnectionFactory.GetAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
                              UPDATE jobs
                              SET server_name = @server_name
                              WHERE id IN (SELECT job.id
                                           FROM jobs job
                                           WHERE (job.server_name IS NULL OR job.server_name NOT IN (SELECT server.server_name FROM servers server))
                                             AND (job.next_occurrence_at IS NULL OR job.next_occurrence_at <= @now)
                                             AND (job.state = ANY (@intermediate_states))
                                           ORDER BY next_occurrence_at
                                               FOR UPDATE SKIP LOCKED
                                           LIMIT @max_rows)
                              RETURNING id, state, created_at, descriptor, server_name, cron, group_name, next_occurrence_at, row_version, errors, context
                              """;
        command.Parameters.Add(new NpgsqlParameter<string>("server_name", serverName));
        command.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("now", _systemClock.UtcNow));
        command.Parameters.Add(new NpgsqlParameter<int[]>("intermediate_states", IntermediateStates));
        command.Parameters.Add(new NpgsqlParameter<int>("max_rows", maxJobsCount));

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var jobs = new List<Job>((int)reader.Rows);

        while (await reader.ReadAsync(cancellationToken))
        {
            var job = ParseJobRow(reader);
            jobs.Add(job);
        }

        return jobs;
    }

    public async Task Add(Job job, CancellationToken cancellationToken = default)
    {
        await using var connection = await _npgsqlConnectionFactory.GetAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
                              insert into jobs (id, state, created_at, descriptor, server_name, cron, group_name, row_version, errors, context)
                              values (@id, @state, @created_at, @descriptor, @server_name, @cron, @group_name, @row_version, @errors, @context);
                              """;

        command.Parameters.Add(new NpgsqlParameter<Guid>("id", job.Id));
        command.Parameters.Add(new NpgsqlParameter<int>("state", (int)job.JobState)); // todo: remove boxing
        command.Parameters.Add(new NpgsqlParameter<DateTimeOffset>("created_at", job.CreatedAt));
        command.Parameters.Add(new NpgsqlParameter<byte[]>("descriptor", _serializer.Serialize(job.Descriptor).ToArray())); // todo: replace ToArray!!
        command.Parameters.Add(new NpgsqlParameter("server_name", job.ServerName is null ? DBNull.Value : job.ServerName)); // todo: usunac nullable - boxing
        command.Parameters.Add(new NpgsqlParameter("cron", job.Cron is null ? DBNull.Value : job.Cron));
        command.Parameters.Add(new NpgsqlParameter("group_name", job.GroupName is null ? DBNull.Value : job.GroupName));
        command.Parameters.Add(new NpgsqlParameter("next_occurrence_at", job.NextOccurrenceAt is null ? DBNull.Value : job.NextOccurrenceAt));
        command.Parameters.Add(new NpgsqlParameter<int>("row_version", job.RowVersion));
        command.Parameters.Add(new NpgsqlParameter<byte[]>("errors", _serializer.Serialize(job.Errors).ToArray()));
        command.Parameters.Add(new NpgsqlParameter<byte[]>("context", _serializer.Serialize(job.Context).ToArray()));

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Job?> Get(Guid jobId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _npgsqlConnectionFactory.GetAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
                              select id, state, created_at, descriptor, server_name, cron, group_name, next_occurrence_at, row_version, errors, context
                              from jobs where id = @id
                              """;
        command.Parameters.Add(new NpgsqlParameter<Guid>("id", jobId));
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ParseJobRow(reader) : null;
    }

    public async Task Update(Guid jobId, Job job, CancellationToken cancellationToken = default)
    {
        await using var connection = await _npgsqlConnectionFactory.GetAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
                              update jobs
                              set
                                  state = @state,
                                  created_at = @created_at,
                                  server_name = @server_name,
                                  cron = @cron,
                                  group_name = @group_name,
                                  next_occurrence_at = @next_occurrence_at,
                                  row_version = @row_version,
                                  errors = @errors,
                                  context = @context
                              where jobs.id = @id;
                              """;
        command.Parameters.Add(new NpgsqlParameter("id", job.Id));
        command.Parameters.Add(new NpgsqlParameter("state", (int)job.JobState)); // todo: remove boxing
        command.Parameters.Add(new NpgsqlParameter<byte[]>("descriptor", _serializer.Serialize(job.Descriptor).ToArray()));
        command.Parameters.Add(new NpgsqlParameter("server_name", job.ServerName is null ? DBNull.Value : job.ServerName));
        command.Parameters.Add(new NpgsqlParameter("cron", job.Cron is null ? DBNull.Value : job.Cron));
        command.Parameters.Add(new NpgsqlParameter("group_name", job.GroupName is null ? DBNull.Value : job.GroupName));
        command.Parameters.Add(new NpgsqlParameter("next_occurrence_at", job.NextOccurrenceAt is null ? DBNull.Value : job.NextOccurrenceAt));
        command.Parameters.Add(new NpgsqlParameter("errors", _serializer.Serialize(job.Errors ?? new List<Exception>())));
        command.Parameters.Add(new NpgsqlParameter("context", _serializer.Serialize(job.Context ?? new Dictionary<string, string>())));

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private Job ParseJobRow(NpgsqlDataReader reader)
    {
        var id = reader.GetGuid(0);
        var state = reader.GetInt32(1);
        var created = (DateTimeOffset)reader.GetDateTime(2);
        var descriptor = _serializer.Deserialize<IJobDescriptor>(reader.GetFieldValue<byte[]>(3));

        string? serverName = null;

        if (!reader.IsDBNull(4))
        {
            serverName = reader.GetString(4);
        }

        string? cron = null;

        if (!reader.IsDBNull(5))
        {
            cron = reader.GetString(5);
        }

        string? groupName = null;

        if (!reader.IsDBNull(6))
        {
            groupName = reader.GetString(6);
        }

        DateTimeOffset? nextOccurrenceAt = null;

        if (!reader.IsDBNull(7))
        {
            nextOccurrenceAt = reader.GetDateTime(7);
        }

        var rowVersion = reader.GetInt32(8);

        var errors = _serializer.Deserialize<List<Exception>>(reader.GetFieldValue<byte[]>(9));
        var context = _serializer.Deserialize<Dictionary<string, string>>(reader.GetFieldValue<byte[]>(10));

        return new Job
        {
            Id = id,
            Descriptor = descriptor,
            JobState = (JobState)state,
            CreatedAt = created,
            Cron = cron,
            ServerName = serverName,
            GroupName = groupName,
            NextOccurrenceAt = nextOccurrenceAt,
            RowVersion = rowVersion,
            Errors = errors,
            Context = context
        };
    }
}