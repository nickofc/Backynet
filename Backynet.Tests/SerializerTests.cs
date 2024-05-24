using Backynet.Abstraction;

namespace Backynet.Tests;

public class SerializerTests
{
    private readonly ISerializer _serializer = new MessagePackSerializerProvider();

    [Fact]
    public void Should_Serialize_With_Valid_Types()
    {
        var dto = new Dto { Username = "Marian" };
        var jobDescriptor = JobDescriptorFactory.Create(() => Run(1, 5, 1_000_000_000, null, dto ));

        var payload = _serializer.Serialize(jobDescriptor.Arguments);
        var deserialized = _serializer.Deserialize<IArgument[]>(payload);

        Assert.Equal(jobDescriptor.Arguments, deserialized);
    }

    private static Task Run(int argA, int argB, long argC, Dto _, Dto dto)
    {
        return Task.CompletedTask;
    }

    private record Dto
    {
        public string Username { get; set; }
    }
}   