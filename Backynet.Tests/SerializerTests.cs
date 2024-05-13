using Backynet.Abstraction;

namespace Backynet.Tests;

public class SerializerTests
{
    private readonly ISerializer _serializer = DefaultJsonSerializer.Instance;

    [Fact]
    public void Should_Serialize_With_Valid_Types()
    {
        var jobDescriptor = JobDescriptorFactory.Create(() => Run(1, 5));

        var payload = _serializer.Serialize(jobDescriptor.Arguments);
        var deserialized = _serializer.Deserialize<IArgument[]>(payload);

        Assert.Equal(jobDescriptor.Arguments, deserialized);
    }

    private static Task Run(int argA, int argB)
    {
        return Task.CompletedTask;
    }
}   