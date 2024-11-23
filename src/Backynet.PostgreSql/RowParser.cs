using Backynet.Abstraction;
using Npgsql;

namespace Backynet.PostgreSql;

internal static class RowParser
{
    public static Job ParseJobRow(ISerializer serializer, NpgsqlDataReader reader)
    {
        var id = reader.GetGuid(0);
        
        var descriptor = serializer.Deserialize<IJobDescriptor>(reader.GetFieldValue<byte[]>(1));
        var context = serializer.Deserialize<Dictionary<string, string>>(reader.GetFieldValue<byte[]>(2));
        var errors = serializer.Deserialize<List<string>>(reader.GetFieldValue<byte[]>(3));
        
        string? cron = null;

        if (!reader.IsDBNull(4))
        {
            cron = reader.GetString(4);
        }
        
        var state = reader.GetInt32(5);
        var created = (DateTimeOffset)reader.GetDateTime(6);
        
        var rowVersion = reader.GetInt32(7);
        
        Guid? lockId = null;

        if (!reader.IsDBNull(8))
        {
            lockId = reader.GetGuid(8);
        }

        DateTimeOffset? lockExpiresAt = null;

        if (!reader.IsDBNull(9))
        {
            lockExpiresAt = reader.GetDateTime(9);
        }
        
        DateTimeOffset? nextOccurrenceAt = null;

        if (!reader.IsDBNull(10))
        {
            nextOccurrenceAt = reader.GetDateTime(10);
        }

        return new Job
        {
            Id = id,
            Descriptor = descriptor,
            JobState = (JobState)state,
            CreatedAt = created,
            Cron = cron,
            RowVersion = rowVersion,
            Errors = errors,
            Context = context,
            LockId = lockId,
            LockExpiresAt = lockExpiresAt,
            NextOccurrenceAt = nextOccurrenceAt
        };
    }
}