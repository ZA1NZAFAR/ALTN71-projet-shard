using Shard.Shared.Core;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Shard.Shared.Web.IntegrationTests.Clock.TaskTracking;

namespace Shard.Shared.Web.IntegrationTests.Clock;

public partial class FakeClock : IClock, IStartupFilter
{
    private readonly AsyncTrackingSyncContext asyncTestSyncContext;
    public AsyncTrackingSyncContext AsyncTestSyncContext => asyncTestSyncContext;

    public FakeClock()
    {
        asyncTestSyncContext = AsyncTrackingSyncContext.Setup();
    }
        
    private interface IEvent
    {
        public DateTime TriggerTime { get; }
        public Task TriggerAsync();
    }

    private readonly ConcurrentDictionary<IEvent, IEvent> events = new();

    private void AddEvent(IEvent anEvent)
        => events.TryAdd(anEvent, anEvent);

    private bool TryRemoveEvent(IEvent anEvent)
        => events.TryRemove(anEvent, out _);

    public DateTime Now { get; private set; }

    public async Task SetNow(DateTime now)
    {
        await TriggerEventsUpTo(now);
        Now = now;
    }

    public Task Advance(TimeSpan timeSpan)
        => SetNow(Now + timeSpan);

    private async Task TriggerEventsUpTo(DateTime now)
    {
        bool hasAnEventBeenTriggered;
        do
        {
            hasAnEventBeenTriggered = await TryTriggerNextEvent(now);
        }
        while (hasAnEventBeenTriggered);
    }

    private async Task<bool> TryTriggerNextEvent(DateTime now)
    {
        var eventToTrigger = FindNextEvent(now);

        if (eventToTrigger == null || !TryRemoveEvent(eventToTrigger))
            return false;

        Now = eventToTrigger.TriggerTime;
        await eventToTrigger.TriggerAsync();
        return true;
    }

    private IEvent? FindNextEvent(DateTime now)
    {
        return events.Values
            .Where(anEvent => anEvent.TriggerTime <= now)
            .OrderBy(anEvent => anEvent.TriggerTime)
            .FirstOrDefault();
    }

    public Task Delay(int millisecondsDelay)
        => Delay(TimeSpan.FromMilliseconds(millisecondsDelay));

    public Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
        => Delay(TimeSpan.FromMilliseconds(millisecondsDelay), cancellationToken);

    public Task Delay(TimeSpan delay)
        => Delay(delay, CancellationToken.None);

    public Task Delay(TimeSpan delay, CancellationToken cancellationToken)
    {
        if (delay.TotalMilliseconds > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(delay), "cannot be be higher than " + int.MaxValue);
        }

        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        if (delay.TotalMilliseconds == 0)
            return Task.CompletedTask;

        if (delay.TotalMilliseconds == -1)
            return InfiniteDelay(cancellationToken);

        if (delay.TotalMilliseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(delay), "cannot be negative except -1 ms");

        var delayEvent = new DelayEvent(Now + delay, asyncTestSyncContext, cancellationToken);
        AddEvent(delayEvent);
        return delayEvent.Task;
    }
    
    private Task InfiniteDelay(CancellationToken cancellationToken)
        => new BaseDelayEvent(asyncTestSyncContext, cancellationToken).Task;

    public void Sleep(int millisecondsTimeout)
        => Delay(millisecondsTimeout).Wait();

    public void Sleep(TimeSpan timeout)
        => Delay(timeout).Wait();

    public ITimer CreateTimer(TimerCallback callback)
        => CreateTimer(callback, this, uint.MaxValue, uint.MaxValue);

    public ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        => CreateTimer(callback, state, (long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds);

    public ITimer CreateTimer(TimerCallback callback, object? state, int dueTime, int period)
    {
        var timer = new Timer(this, callback, state);
        timer.Change(dueTime, period);
        return timer;
    }

    public ITimer CreateTimer(TimerCallback callback, object? state, long dueTime, long period)
    {
        var timer = new Timer(this, callback, state);
        timer.Change(dueTime, period);
        return timer;
    }

    public ITimer CreateTimer(TimerCallback callback, object? state, uint dueTime, uint period)
    {
        var timer = new Timer(this, callback, state);
        timer.Change(dueTime, period);
        return timer;
    }

    Action<IApplicationBuilder> IStartupFilter.Configure(Action<IApplicationBuilder> next)
    {
        return builder =>
        {
            builder.Use(async (context, next) =>
            {
                EnsureIsRunningUnderSynchronizationContext();
                await next.Invoke();
            });
            next(builder);
        };
    }

    private void EnsureIsRunningUnderSynchronizationContext()
    {
        if (SynchronizationContext.Current == null)
            SynchronizationContext.SetSynchronizationContext(asyncTestSyncContext);
    }
}