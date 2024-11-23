using System.Threading.Channels;

namespace Backynet.PostgreSql;

internal sealed class PostgreSqlWorker
{
    private readonly PostgreSqlJobDataProducer _postgreSqlJobDataProducer;
    private readonly PostgreSqlJobDataConsumer _postgreSqlJobDataConsumer;
    private readonly Channel<PostgreSqlJobDataProducer.JobScope> _channel;

    public PostgreSqlWorker(PostgreSqlJobDataProducer postgreSqlJobDataProducer, PostgreSqlJobDataConsumer postgreSqlJobDataConsumer)
    {
        _postgreSqlJobDataProducer = postgreSqlJobDataProducer;
        _postgreSqlJobDataConsumer = postgreSqlJobDataConsumer;

        _channel = Channel.CreateBounded<PostgreSqlJobDataProducer.JobScope>(10);
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        var producerTask = _postgreSqlJobDataProducer.Start(_channel.Writer, cancellationToken);
        var consumerTask = _postgreSqlJobDataConsumer.Start(_channel.Reader, cancellationToken);

        await await Task.WhenAny(producerTask, consumerTask);
    }
}