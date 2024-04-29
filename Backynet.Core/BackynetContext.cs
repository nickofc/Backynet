using Backynet.Core.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace Backynet.Core;

public class BackynetContext
{
    private readonly Guid _contextId = Guid.NewGuid();

    private BackynetContextOptions _options;
    private IServiceCollection? _services;
    private IServiceProvider? _serviceProvider;
    private IServiceScope? _serviceScope;
    private IBackynetContextServices? _contextServices;
    private bool _initializing;
    private bool _disposed;

    private IBackynetServer? _backynetServer;
    private IBackynetClient? _backynetClient;

    public BackynetContext(BackynetContextOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    public IBackynetServer Server
    {
        get
        {
            if (_backynetServer == null)
            {
                _backynetServer = InternalServiceProvider.GetRequiredService<IBackynetServer>();
            }

            return _backynetServer;
        }
    }

    public IBackynetClient Client
    {
        get
        {
            if (_backynetClient == null)
            {
                _backynetClient = InternalServiceProvider.GetRequiredService<IBackynetClient>();
            }

            return _backynetClient;
        }
    }

    private IServiceProvider InternalServiceProvider
        => ContextServices.InternalServiceProvider;

    private IBackynetContextServices ContextServices
    {
        get
        {
            if (_contextServices != null)
            {
                return _contextServices;
            }

            if (_initializing)
            {
                throw new InvalidOperationException();
            }

            try
            {
                _initializing = true;

                var optionsBuilder = new BackynetContextOptionsBuilder(_options);

                OnConfiguring(optionsBuilder);

                _serviceProvider = _services.BuildServiceProvider();
                _serviceScope = _serviceProvider.CreateScope();

                var contextServices = _serviceScope.ServiceProvider.GetRequiredService<IBackynetContextServices>();

                contextServices.Initialize(_serviceProvider);

                _contextServices = contextServices;
            }
            finally
            {
                _initializing = false;
            }

            return _contextServices;
        }
    }

    public virtual void OnConfiguring(BackynetContextOptionsBuilder optionsBuilder)
    {
    }
}