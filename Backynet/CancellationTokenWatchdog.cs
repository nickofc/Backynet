namespace Backynet;

public class CancellationTokenWatchdog
{
    public async Task Start(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            
            
        }
    }
}
