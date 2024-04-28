namespace Backynet.Core;

public interface IDbContextOptionsBuilderInfrastructure
{
    void AddOrUpdateExtension<TExtension>(TExtension extension)
        where TExtension : class, IBackynetContextOptionsExtension;
}