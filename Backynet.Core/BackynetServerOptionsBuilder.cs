namespace Backynet.Core;

public class BackynetServerOptionsBuilder
{
    public BackynetServerOptions Options { get; }

    public BackynetServerOptionsBuilder(BackynetServerOptions options)
    {
        Options = options;
    }

    public BackynetServerOptionsBuilder() : this(new BackynetServerOptions())
    {
        
    }

    public BackynetServerOptionsBuilder UseMaximumConcurrencyThreads(int maximumConcurrencyThreads)
    {
        Options.MaxThreads = maximumConcurrencyThreads;
        return this;
    }

    public BackynetServerOptionsBuilder UseServerName(string serverName)
    {
        Options.ServerName = serverName;
        return this;
    }
}