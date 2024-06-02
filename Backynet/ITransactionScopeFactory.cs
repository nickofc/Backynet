namespace Backynet;

public interface ITransactionScopeFactory
{
    ITransactionScope BeginAsync();
}