using Backynet.Options;

namespace Backynet.PostgreSql;

internal interface IPostgreSqlBackynetContextOptionsBuilderInfrastructure
{
    BackynetContextOptionsBuilder OptionsBuilder { get; }
}