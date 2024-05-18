using System.Buffers.Text;
using System.Text;
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
                              RETURNING id, state, created_at, base_type, method, arguments, server_name, cron, group_name, next_occurrence_at, row_version, errors, context
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
                              insert into jobs (id, state, created_at, base_type, method, arguments, server_name, cron, group_name, row_version, errors, context)
                              values (@id, @state, @created_at, @base_type, @method, @arguments, @server_name, @cron, @group_name, @row_version, @errors, @context);
                              """;
        command.Parameters.Add(new NpgsqlParameter("id", job.Id));
        command.Parameters.Add(new NpgsqlParameter("state", (int)job.JobState)); // todo: remove boxing
        command.Parameters.Add(new NpgsqlParameter("created_at", job.CreatedAt));
        command.Parameters.Add(new NpgsqlParameter("base_type", job.Descriptor.Method.TypeName));
        command.Parameters.Add(new NpgsqlParameter("method", job.Descriptor.Method.Name));
        command.Parameters.Add(new NpgsqlParameter("arguments", _serializer.Serialize(job.Descriptor.Arguments)));
        command.Parameters.Add(new NpgsqlParameter("server_name", job.ServerName is null ? DBNull.Value : job.ServerName));
        command.Parameters.Add(new NpgsqlParameter("cron", job.Cron is null ? DBNull.Value : job.Cron));
        command.Parameters.Add(new NpgsqlParameter("group_name", job.GroupName is null ? DBNull.Value : job.GroupName));
        command.Parameters.Add(new NpgsqlParameter("next_occurrence_at", job.NextOccurrenceAt is null ? DBNull.Value : job.NextOccurrenceAt));
        command.Parameters.Add(new NpgsqlParameter<int>("row_version", job.RowVersion));
        command.Parameters.Add(new NpgsqlParameter("errors", _serializer.Serialize(job.Errors ?? new List<Exception>())));
        command.Parameters.Add(new NpgsqlParameter("context", _serializer.Serialize(job.Context ?? new Dictionary<string, string>())));

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Job?> Get(Guid jobId, CancellationToken cancellationToken = default)
    {
        await using var connection = await _npgsqlConnectionFactory.GetAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
                              select id, state, created_at, base_type, method, arguments, server_name, cron, group_name, next_occurrence_at, row_version, errors, context
                              from jobs where id = @id
                              """;
        command.Parameters.Add(new NpgsqlParameter("id", jobId));

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var read = await reader.ReadAsync(cancellationToken);
        Job? job = null;

        if (read)
        {
            job = ParseJobRow(reader);
        }

        return job;
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
                                  base_type = @base_type,
                                  method = @method,
                                  arguments = @arguments,
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
        command.Parameters.Add(new NpgsqlParameter("created_at", job.CreatedAt));
        command.Parameters.Add(new NpgsqlParameter("base_type", job.Descriptor.Method.TypeName));
        command.Parameters.Add(new NpgsqlParameter("method", job.Descriptor.Method.Name));
        command.Parameters.Add(new NpgsqlParameter("arguments", _serializer.Serialize(job.Descriptor.Arguments)));
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
        var created = reader.GetDateTime(2);

        var baseType = reader.GetString(3);
        var methodName = reader.GetString(4);
        var arguments = _serializer.Deserialize<IArgument[]>(GetData(reader.GetStream(5)));

        string? serverName = null;

        if (!reader.IsDBNull(6))
        {
            serverName = reader.GetString(6);
        }

        string? cron = null;

        if (!reader.IsDBNull(7))
        {
            cron = reader.GetString(7);
        }

        string? groupName = null;

        if (!reader.IsDBNull(8))
        {
            groupName = reader.GetString(8);
        }

        DateTimeOffset? nextOccurrenceAt = null;

        if (!reader.IsDBNull(9))
        {
            nextOccurrenceAt = reader.GetDateTime(9);
        }

        var method = new Method(baseType, methodName);

        var rowVersion = reader.GetInt32(10);

        var errors = _serializer.Deserialize<List<Exception>>(GetData(reader.GetStream(11)));
        var context = _serializer.Deserialize<Dictionary<string, string>>(GetData(reader.GetStream(12)));

        return new Job
        {
            Id = id,
            JobState = (JobState)state,
            CreatedAt = created,
            Descriptor = new JobDescriptor(method, arguments),
            Cron = cron,
            ServerName = serverName,
            GroupName = groupName,
            NextOccurrenceAt = nextOccurrenceAt,
            RowVersion = rowVersion,
            Errors = errors,
            Context = context
        };

        ReadOnlyMemory<byte> GetData(Stream inputStream)
        {
            using var memoryStream = new MemoryStream((int)inputStream.Length);
            inputStream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}