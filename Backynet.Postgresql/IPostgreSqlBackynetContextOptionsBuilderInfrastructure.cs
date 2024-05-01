using Backynet.Core;

namespace Backynet.PostgreSql;

internal interface IPostgreSqlBackynetContextOptionsBuilderInfrastructure
{
    BackynetContextOptionsBuilder OptionsBuilder { get; }
}