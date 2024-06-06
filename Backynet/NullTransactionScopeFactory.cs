namespace Backynet;

public class NullTransactionScopeFactory : ITransactionScopeFactory
{
    public ITransactionScope BeginAsync()
    {
        return new NullTransactionScope();
    }
}