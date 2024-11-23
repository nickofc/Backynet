using System.Threading.Channels;

namespace Backynet.PostgreSql;

internal sealed class PostgreSqlJobDataConsumer
{
    private readonly IJobExecutor _jobExecutor;

    public PostgreSqlJobDataConsumer(IJobExecutor jobExecutor)
    {
        _jobExecutor = jobExecutor;
    }

    public async Task Start(ChannelReader<PostgreSqlJobDataProducer.JobScope> channelReader, CancellationToken cancellationToken)
    {
        await foreach (var jobScope in channelReader.ReadAllAsync(cancellationToken))
        {
            using var _ = jobScope;
            await _jobExecutor.Execute(jobScope.Job, jobScope.CancellationToken);
        }
    }
}