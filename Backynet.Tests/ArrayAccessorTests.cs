namespace Backynet.Tests;

public class ArrayAccessorTests
{
    [Fact]
    public void Should_Return_Internal_Array()
    {
        // arrange

        var expectedList = Enumerable.Range(0, 100)
            .Select(_ => Random.Shared.Next(0, 100))
            .ToList();

        // act

        var actualList = expectedList.GetInternalArray();

        // assert

        Assert.Equal(expectedList, actualList);
    }
}