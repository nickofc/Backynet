using Backynet.Core;
using Backynet.PostgreSql;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static BackynetServerOptionsBuilder UsePostgreSql(
        this BackynetServerOptionsBuilder optionsBuilder,
        Action<BackynetServerPostgreSqlOptionsBuilder> configure)
    {
        return optionsBuilder;
    }
}