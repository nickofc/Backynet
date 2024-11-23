using Backynet.Abstraction;
using Npgsql;

namespace Backynet.PostgreSql;

internal sealed class PostgreSqlJobQueue
{
    private readonly NpgsqlConnectionFactory _npgsqlConnectionFactory;
    private readonly ISerializer _serializer;
    private readonly PostgreSqlOptions _postgreSqlOptions;

    public PostgreSqlJobQueue(
        NpgsqlConnectionFactory npgsqlConnectionFactory,
        ISerializer serializer,
        PostgreSqlOptions postgreSqlOptions)
    {
        _npgsqlConnectionFactory = npgsqlConnectionFactory;
        _serializer = serializer;
        _postgreSqlOptions = postgreSqlOptions;
    }

    public async Task<IReadOnlyCollection<Job>> Fetch(int fetchSize, CancellationToken cancellationToken = default)
    {
        var sql = $"""
                   WITH subquery AS (SELECT *
                                     FROM {_postgreSqlOptions.Schema}.jobs job
                                     WHERE (job.lock_id IS NULL OR job.lock_expires_at < CURRENT_TIMESTAMP)
                                     LIMIT @fetch_size FOR UPDATE SKIP LOCKED)

                   UPDATE {_postgreSqlOptions.Schema}.jobs job
                   SET lock_id = gen_random_uuid(),
                       lock_expires_at = CURRENT_TIMESTAMP + @lock_duration
                   FROM subquery
                   WHERE job.id = subquery.id
                   RETURNING job.id,
                     job.descriptor,
                     job.context,
                     job.errors,
                     job.cron,
                     job.state,
                     job.created_at,
                     job.row_version,
                     job.lock_id,
                     job.lock_expires_at,
                     job.next_occurrence_at
                   """;

        await using var connection = _npgsqlConnectionFactory.Get();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(new NpgsqlParameter<int>("fetch_size", fetchSize));
        command.Parameters.Add(new NpgsqlParameter<TimeSpan>("lock_duration", _postgreSqlOptions.LockDuration));
        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var jobs = new List<Job>((int)reader.Rows);

        while (await reader.ReadAsync(cancellationToken))
        {
            var job = RowParser.ParseJobRow(_serializer, reader);
            jobs.Add(job);
        }

        return jobs;
    }

    public async Task<IReadOnlyCollection<Job>> Refresh(IReadOnlyCollection<Job> jobs, CancellationToken cancellationToken)
    {
        var sql = $"""
                   UPDATE {_postgreSqlOptions.Schema}.jobs job
                   SET lock_expires_at = CURRENT_TIMESTAMP + @lock_duration
                   WHERE job.id = ANY(@job_ids) AND job.lock_id = ANY(@lock_ids)
                   RETURNING job.id, job.descriptor, job.context, job.errors, job.cron,
                             job.state, job.created_at, job.row_version, job.lock_id, 
                             job.lock_expires_at, job.next_occurrence_at
                   """;

        var jobIds = jobs.Select(x => x.Id).ToArray();
        var lockIds = jobs.Select(x => x.LockId ?? throw new InvalidOperationException("")).ToArray();

        await using var connection = _npgsqlConnectionFactory.Get();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add(new NpgsqlParameter<TimeSpan>("lock_duration", _postgreSqlOptions.LockDuration));
        command.Parameters.Add(new NpgsqlParameter<Guid[]>("job_ids", jobIds));
        command.Parameters.Add(new NpgsqlParameter<Guid[]>("lock_ids", lockIds));

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var actualJobs = new List<Job>((int)reader.Rows);

        while (await reader.ReadAsync(cancellationToken))
        {
            var job = RowParser.ParseJobRow(_serializer, reader);
            actualJobs.Add(job);
        }

        return actualJobs;
    }
}