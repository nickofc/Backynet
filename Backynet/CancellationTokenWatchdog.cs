using System.Collections.Concurrent;

namespace Backynet;

public class CancellationTokenWatchdog
{
    private readonly ConcurrentDictionary<Guid, CancellationToken> _tokens = new();
    
    public async Task Start(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    public CancellationToken GetCancellationToken(Guid jobId)
    {
        var newToken = new CancellationToken();
        _tokens[jobId] = newToken;
        return newToken;
    }
}
