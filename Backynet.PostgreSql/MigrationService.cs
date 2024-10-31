using System.Diagnostics;
using System.Reflection;

namespace Backynet.PostgreSql;

internal sealed class MigrationService
{
    private static readonly Assembly MigrationAssembly = typeof(MigrationService).Assembly;

    private readonly NpgsqlConnectionFactory _connectionFactory;
    private readonly MigrationServiceOptions _migrationServiceOptions;

    public MigrationService(NpgsqlConnectionFactory connectionFactory, MigrationServiceOptions migrationServiceOptions)
    {
        _connectionFactory = connectionFactory;
        _migrationServiceOptions = migrationServiceOptions;
    }

    public async Task Perform(CancellationToken cancellationToken = default)
    {
        await using var stream = MigrationAssembly.GetManifestResourceStream("Backynet.PostgreSql.migration.sql");
        Debug.Assert(stream != null, "database migration file was not found"); 
        using var streamReader = new StreamReader(stream);
        await using var connection = _connectionFactory.Get();
        await using var command = connection.CreateCommand();
        command.CommandText = await streamReader.ReadToEndAsync(cancellationToken);
        command.Parameters.AddWithValue("@coo", _migrationServiceOptions.SearchPath);
        await connection.OpenAsync(cancellationToken);
        var res = await command.ExecuteScalarAsync(cancellationToken);
    }
}

internal sealed class MigrationServiceOptions
{
    public string SearchPath { get; set; }
}