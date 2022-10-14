namespace Shard.Shared.Web.IntegrationTests;

public class TimeoutHandler : DelegatingHandler
{
    public TimeoutHandler()
    {
    }

    public TimeoutHandler(HttpMessageHandler innerHandler) : base(innerHandler)
    {
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<HttpResponseMessage>(cancellationToken);

        return WrapTaskWithCancellationToken(
            base.SendAsync(request, cancellationToken),
            cancellationToken);
    }

    private static async Task<HttpResponseMessage> WrapTaskWithCancellationToken(Task<HttpResponseMessage> task, CancellationToken cancellationToken)
        => await await Task.WhenAny(task, CreateTaskForCancellationToken(cancellationToken));

    private static Task<HttpResponseMessage> CreateTaskForCancellationToken(CancellationToken cancellationToken)
    {
        TaskCompletionSource<HttpResponseMessage> taskCompletionSource = new();
        cancellationToken.Register(() => taskCompletionSource.TrySetCanceled(cancellationToken));
        return taskCompletionSource.Task;
    }
}
