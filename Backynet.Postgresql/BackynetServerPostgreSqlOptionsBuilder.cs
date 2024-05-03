using Backynet.Options;

namespace Backynet.PostgreSql;

public class PostgreSqlBackynetContextOptionsBuilder : IPostgreSqlBackynetContextOptionsBuilderInfrastructure
{
    public PostgreSqlBackynetContextOptionsBuilder(BackynetContextOptionsBuilder optionsBuilder)
    {
        OptionsBuilder = optionsBuilder;
    }

    protected virtual BackynetContextOptionsBuilder OptionsBuilder { get; }

    BackynetContextOptionsBuilder IPostgreSqlBackynetContextOptionsBuilderInfrastructure.OptionsBuilder => OptionsBuilder;

    public virtual PostgreSqlBackynetContextOptionsBuilder UseCommandTimeout(TimeSpan? commandTimeout)
    {
        var extension = OptionsBuilder.Options.FindExtension<PostgreSqlOptionsExtension>()
                        ?? new PostgreSqlOptionsExtension();

        extension = extension.WithCommandTimeout(commandTimeout);

        ((IBackynetContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(extension);

        return this;
    }

    public virtual PostgreSqlBackynetContextOptionsBuilder UseAutomaticMigration(bool? isAutomaticMigrationEnabled)
    {
        var extension = OptionsBuilder.Options.FindExtension<PostgreSqlOptionsExtension>()
                        ?? new PostgreSqlOptionsExtension();

        extension = extension.WithAutomaticMigration(isAutomaticMigrationEnabled);

        ((IBackynetContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(extension);

        return this;
    }
}