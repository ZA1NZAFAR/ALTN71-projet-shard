namespace Shard.Shared.Core;

public class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;

    public ITimer CreateTimer(TimerCallback callback)
        => new WrappingTimer(new(callback));

    public ITimer CreateTimer(TimerCallback callback, object? state, int dueTime, int period)
        => new WrappingTimer(new(callback, state, dueTime, period));

    public ITimer CreateTimer(TimerCallback callback, object? state, long dueTime, long period)
        => new WrappingTimer(new(callback, state, dueTime, period));

    public ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        => new WrappingTimer(new(callback, state, dueTime, period));

    public ITimer CreateTimer(TimerCallback callback, object? state, uint dueTime, uint period)
        => new WrappingTimer(new(callback, state, dueTime, period));

    public Task Delay(int millisecondsDelay)
        => Task.Delay(millisecondsDelay);
    public Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
        => Task.Delay(millisecondsDelay, cancellationToken);
    public Task Delay(TimeSpan delay)
        => Task.Delay(delay);
    public Task Delay(TimeSpan delay, CancellationToken cancellationToken)
        => Task.Delay(delay, cancellationToken);

    public void Sleep(int millisecondsTimeout)
        => Thread.Sleep(millisecondsTimeout);
    public void Sleep(TimeSpan timeout)
        => Thread.Sleep(timeout);
}
