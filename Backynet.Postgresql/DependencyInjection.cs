using Backynet.Core;
using Backynet.PostgreSql;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

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

        extension.WithConnectionString(connectionString);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        configure?.Invoke(new PostgreSqlBackynetContextOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }
}