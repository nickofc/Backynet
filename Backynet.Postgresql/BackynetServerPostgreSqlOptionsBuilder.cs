using Backynet.Core;

namespace Backynet.PostgreSql;

public class PostgreSqlBackynetContextOptionsBuilder : IPostgreSqlBackynetContextOptionsBuilderInfrastructure
{
    public PostgreSqlBackynetContextOptionsBuilder(BackynetContextOptionsBuilder optionsBuilder)
    {
        OptionsBuilder = optionsBuilder;
    }

    public BackynetContextOptionsBuilder OptionsBuilder { get; }
}