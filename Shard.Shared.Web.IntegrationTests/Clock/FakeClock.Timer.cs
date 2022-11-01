using Shard.Shared.Core;

namespace Shard.Shared.Web.IntegrationTests.Clock;

public partial class FakeClock
{
    public sealed class Timer : ITimer, IEvent
    {
        private readonly FakeClock clock;
        private readonly TimerCallback callback;
        private readonly object? state;

        private DateTime lastChangeTime;
        private uint dueTime = uint.MaxValue;
        private uint period = uint.MaxValue;

        public Timer(FakeClock clock, TimerCallback callback, object? state)
        {
            this.clock = clock;
            this.callback = callback;
            this.state = state;
        }

        public DateTime TriggerTime
            => lastChangeTime.AddMilliseconds(dueTime);

        public bool Change(TimeSpan dueTime, TimeSpan period)
            => Change((long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds);

        public bool Change(int dueTime, int period)
        {
            if (dueTime < -1)
                throw new ArgumentOutOfRangeException(nameof(dueTime), "cannot be lower than -1");
            if (period < -1)
                throw new ArgumentOutOfRangeException(nameof(period), "cannot be lower than -1");
            return Change((uint) dueTime, (uint) period);
        }

        public bool Change(long dueTime, long period)
        {
            if (dueTime < -1)
                throw new ArgumentOutOfRangeException(nameof(dueTime), "cannot be lower than -1");
            if (period < -1)
                throw new ArgumentOutOfRangeException(nameof(period), "cannot be lower than -1");
            if (dueTime >= uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(dueTime), "cannot exceed " + uint.MaxValue);
            if (period >= uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(period), "cannot exceed " + uint.MaxValue);
            return Change((uint) dueTime, (uint) period);
        }

        public bool Change(uint dueTime, uint period)
        {
            lastChangeTime = clock.Now;

            this.dueTime = dueTime;
            this.period = period != 0 ? period : uint.MaxValue;

            if (this.dueTime == 0)
                Trigger();
            else
                clock.AddEvent(this);

            return true;
        }

        public void Trigger()
        {
            callback(state);
            Change(period, period);
        }

        public Task TriggerAsync()
        {
            Trigger();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            clock.TryRemoveEvent(this);
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return new ValueTask(Task.CompletedTask);
        }
    }
}
