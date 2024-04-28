using Backynet.Core;

namespace Backynet.PostgreSql;

public interface IPostgreSqlBackynetContextOptionsBuilderInfrastructure
{
    BackynetContextOptionsBuilder OptionsBuilder { get; }
}