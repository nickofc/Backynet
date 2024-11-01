namespace Backynet.Options;

public interface IBackynetContextOptionsBuilderInfrastructure
{
    void AddOrUpdateExtension<TExtension>(TExtension extension) where TExtension : class, IBackynetContextOptionsExtension;
}