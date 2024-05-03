using System.Reflection;
using Backynet.Abstraction;
using JetBrains.dotMemoryUnit;
using Xunit.Abstractions;

namespace Backynet.Tests;

public class CronTests
{
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;
    private readonly Assembly[] _assemblies = [typeof(IBackynetClient).Assembly];

    public CronTests(ITestOutputHelper output)
    {
        DotMemoryUnitTestOutput.SetOutputMethod(output.WriteLine);
    }

    [Fact(Skip = "wip")]
    [DotMemoryUnit(CollectAllocations = true)]
    public void Should_Not_Allocate_Memory()
    {
        var startingCheckpoint = dotMemory.Check();

        var cron = Cron.Parse("1 * * * *");
        cron.GetNextOccurrence(_now);

        dotMemory.Check(memory =>
        {
            var trafficFrom = memory.GetDifference(startingCheckpoint);
            var survivedObjects = trafficFrom.GetSurvivedObjects(x => x.Assembly.Is(_assemblies));

            Assert.Equal(0, survivedObjects.ObjectsCount);
        });
    }

    [Fact(Skip = "wip")]
    public void Should_Parse()
    {
        var cron = Cron.Parse("30 * * * *");
        cron.GetNextOccurrence(_now);
    }
}