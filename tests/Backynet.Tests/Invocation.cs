namespace Backynet.Tests;

public static class Invocation
{
    public static Action? OnFakeMethodInvocation { get; set; }

    public static void FakeMethod()
    {
        OnFakeMethodInvocation?.Invoke();
    }

    public static Action? OnFakeAsyncMethodInvocation { get; set; }

    public static async Task FakeAsyncMethod()
    {
        OnFakeAsyncMethodInvocation?.Invoke();
        await Task.CompletedTask;
    }

    public static Action? OnFakeInfinityMethodWithCancellationTokenInvocation { get; set; }
    public static Action<CancellationToken>? OnFakeInfinityMethodWithCancellationTokenCancelledInvocation { get; set; }

    public static async Task FakeInfinityMethodWithCancellationToken(CancellationToken cancellationToken = default)
    {
        OnFakeInfinityMethodWithCancellationTokenInvocation?.Invoke();

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            OnFakeInfinityMethodWithCancellationTokenCancelledInvocation?.Invoke(cancellationToken);
        }
    }
}