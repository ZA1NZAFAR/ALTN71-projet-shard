namespace Shard.Shared.Core;

public interface IClock
{
    DateTime Now { get; }

    ITimer CreateTimer(TimerCallback callback);
    ITimer CreateTimer(TimerCallback callback, object? state, int dueTime, int period);
    ITimer CreateTimer(TimerCallback callback, object? state, long dueTime, long period);
    ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period);
    ITimer CreateTimer(TimerCallback callback, object? state, uint dueTime, uint period);
    Task Delay(int millisecondsDelay);
    Task Delay(int millisecondsDelay, CancellationToken cancellationToken);
    Task Delay(TimeSpan delay);
    Task Delay(TimeSpan delay, CancellationToken cancellationToken);
    void Sleep(int millisecondsTimeout);
    void Sleep(TimeSpan timeout);
}
