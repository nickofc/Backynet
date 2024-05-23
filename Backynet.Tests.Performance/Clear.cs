using Npgsql;

namespace Backynet.Tests.Performance;

public static class Clear
{
    public static void HangfireDatabase()
    {
        using var connection = new NpgsqlConnection(TestContext.ConnectionString);
        using var command = connection.CreateCommand();
        command.CommandText = "delete from hangfire.job where 1=1";
        connection.Open();
        command.ExecuteNonQuery();
    }

    public static void BackynetDatabase()
    {
        using var connection = new NpgsqlConnection(TestContext.ConnectionString);
        using var command = connection.CreateCommand();
        command.CommandText = "delete from public.jobs where 1=1";
        connection.Open();
        command.ExecuteNonQuery();
    }
}