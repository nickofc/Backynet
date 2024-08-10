using Backynet.Tests;

namespace Backynet.PostgreSql.Tests;

public class Test
{
    [Fact(Timeout = 1000 * 5)]
    public async Task Should()
    {
        const string expectedChannelName = "backynet";
        const string expectedPayload = """
                                       {"menu": {
                                         "id": "file",
                                         "value": "File",
                                         "popup": {
                                           "menuitem": [
                                             {"value": "New", "onclick": "CreateNewDoc()"},
                                             {"value": "Open", "onclick": "OpenDoc()"},
                                             {"value": "Close", "onclick": "CloseDoc()"}
                                           ]
                                         }
                                       }}
                                       """;

        var completionSource = new TaskCompletionSource<(string, string)>();
        using var tokenSource = new CancellationTokenSource();

        var callbackMap = new Dictionary<string, List<Func<string, string, CancellationToken, Task>>>
        {
            { expectedChannelName, [Callback] }
        };

        var reader = new Reader(TestContext.ConnectionString);
        var readerTask = reader.Start(callbackMap, tokenSource.Token);

        await Task.WhenAny(readerTask, Task.Delay(1, CancellationToken.None));

        var writer = new Writer(TestContext.ConnectionString);
        await writer.Send(expectedChannelName, expectedPayload, tokenSource.Token);

        var (actualChannelName, actualPayload) = await completionSource.Task;

        Assert.Equal(expectedChannelName, actualChannelName);
        Assert.Equal(expectedPayload, actualPayload);

        return;

        Task Callback(string channelName, string payload, CancellationToken cancellationToken)
        {
            completionSource.SetResult((channelName, payload));
            return Task.CompletedTask;
        }
    }
}