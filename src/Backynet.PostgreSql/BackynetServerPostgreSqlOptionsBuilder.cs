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
        return WithOption(x => x.WithCommandTimeout(commandTimeout));
    }

    public virtual PostgreSqlBackynetContextOptionsBuilder UseAutomaticMigration(bool? isAutomaticMigrationEnabled)
    {
        return WithOption(x => x.WithAutomaticMigration(isAutomaticMigrationEnabled));
    }

    public virtual PostgreSqlBackynetContextOptionsBuilder UseSchema(string? schema)
    {
        return WithOption(x => x.WithSchema(schema));
    }

    private PostgreSqlBackynetContextOptionsBuilder WithOption(Func<PostgreSqlOptionsExtension, PostgreSqlOptionsExtension> withFunc)
    {
        ((IBackynetContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(
            withFunc(OptionsBuilder.Options.FindExtension<PostgreSqlOptionsExtension>() ?? new PostgreSqlOptionsExtension()));

        return this;
    }
}