namespace Backynet.Core;

public class BackynetHost
{
    private readonly BackynetHostOptions _backynetHostOptions;
    private readonly IControllerService _controllerService;

    public BackynetHost(IControllerService controllerService, BackynetHostOptions backynetHostOptions)
    {
        _controllerService = controllerService;
        _backynetHostOptions = backynetHostOptions;
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        await Heartbeat(cancellationToken);


        // todo 
        // jak wybrać mastera?
    }

    public bool IsMaster { get; set; }

    public async Task Heartbeat(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _controllerService.Heartbeat(_backynetHostOptions.ServerName, cancellationToken);
            await _controllerService.Purge(cancellationToken);
            await Task.Delay(_backynetHostOptions.HeartbeatInterval, cancellationToken);
        }
    }
}