using System.Threading.Channels;
using Backynet.Abstraction;

namespace Backynet.PostgreSql;

internal sealed class PostgreSqlJobDataProducer
{
    private readonly PostgreSqlJobQueue _postgreSqlJobQueue;
    private readonly PostgreSqlOptions _postgreSqlOptions;
    private readonly List<Job> _jobs;
    private readonly object _syncRoot;

    public PostgreSqlJobDataProducer(PostgreSqlJobQueue postgreSqlJobQueue, PostgreSqlOptions postgreSqlOptions)
    {
        _postgreSqlJobQueue = postgreSqlJobQueue;
        _postgreSqlOptions = postgreSqlOptions;
        _jobs = [];
        _syncRoot = new object();
    }

    public async Task Start(ChannelWriter<JobScope> channelWriter, CancellationToken cancellationToken)
    {
        await await Task.WhenAny(Producer(channelWriter, cancellationToken), Watchdog(cancellationToken));
    }

    private async Task Producer(ChannelWriter<JobScope> channelWriter, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var jobs = await _postgreSqlJobQueue.Fetch(_postgreSqlOptions.FetchSize, cancellationToken);

            if (jobs.Count == 0)
            {
                await Task.Delay(_postgreSqlOptions.PoolingInterval, cancellationToken);
                continue;
            }

            lock (_syncRoot) // todo: czy ten lock jest wymagany? 
            {
                foreach (var job in jobs)
                {
                    _jobs.Add(job);
                }
            }

            foreach (var job in jobs)
            {
                var jobScope = new JobScope(job, this, cancellationToken);

                await channelWriter.WriteAsync(jobScope, cancellationToken);
            }
        }

        // ReSharper disable once FunctionNeverReturns
    }

    private void Release(JobScope jobScope)
    {
        lock (_syncRoot)
        {
            _jobs.Remove(jobScope.Job);
        }
    }

    private async Task Watchdog(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // ReSharper disable once InconsistentlySynchronizedField
            await _postgreSqlJobQueue.Refresh(_jobs, cancellationToken);
            await Task.Delay(_postgreSqlOptions.RefreshInterval, cancellationToken);
        }

        // ReSharper disable once FunctionNeverReturns
    }

    internal sealed class JobScope : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly PostgreSqlJobDataProducer _postgreSqlJobDataProducer;

        public JobScope(Job job, PostgreSqlJobDataProducer postgreSqlJobDataProducer, CancellationToken cancellationToken)
        {
            _postgreSqlJobDataProducer = postgreSqlJobDataProducer;
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            Job = job;
            CancellationToken = _cancellationTokenSource.Token;
        }

        public Job Job { get; }
        public CancellationToken CancellationToken { get; }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            _postgreSqlJobDataProducer.Release(this);
            _cancellationTokenSource.Dispose();
        }
    }
}