using Backynet.PostgreSql;

// ReSharper disable once CheckNamespace
namespace Backynet.Core;

public static class DependencyInjection
{
    public static BackynetContextOptionsBuilder UsePostgreSql(
        this BackynetContextOptionsBuilder optionsBuilder,
        string connectionString,
        Action<PostgreSqlBackynetContextOptionsBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentNullException.ThrowIfNull(connectionString);

        var extension = optionsBuilder.Options.FindExtension<PostgreSqlOptionsExtension>() ?? new PostgreSqlOptionsExtension();

        extension = extension.WithConnectionString(connectionString);

        ((IBackynetContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        configure?.Invoke(new PostgreSqlBackynetContextOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }
}