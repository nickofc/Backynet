using Backynet.Abstraction;
using Backynet.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Backynet;

public class BackynetContext
{
    private readonly BackynetContextOptions _options;
    private IServiceScope? _serviceScope;
    private IBackynetContextServices? _contextServices;
    private bool _initializing;

    private IBackynetServer? _backynetServer;
    private IBackynetClient? _backynetClient;

    protected BackynetContext() : this(new BackynetContextOptions<BackynetContext>())
    {
    }

    public BackynetContext(BackynetContextOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!options.ContextType.IsAssignableFrom(GetType()))
        {
            throw new InvalidOperationException();
        }

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

                _serviceScope = ServiceProviderFactory.Create(optionsBuilder.Options).CreateScope();

                var contextServices = _serviceScope.ServiceProvider.GetRequiredService<IBackynetContextServices>();
                contextServices.Initialize(_serviceScope.ServiceProvider, optionsBuilder.Options, this);

                _contextServices = contextServices;

                var loggerFactory = _options.FindExtension<CoreOptionsExtension>()?.LoggerFactory ?? NullLoggerFactory.Instance;
                var logger = loggerFactory.CreateLogger<BackynetContext>();

                logger.ContextCreated(nameof(BackynetContext));
            }
            finally
            {
                _initializing = false;
            }

            return _contextServices;
        }
    }

    protected internal virtual void OnConfiguring(BackynetContextOptionsBuilder optionsBuilder)
    {
    }
}