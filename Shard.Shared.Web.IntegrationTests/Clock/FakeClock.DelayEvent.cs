using Shard.Shared.Web.IntegrationTests.Clock.TaskTracking;
namespace Shard.Shared.Web.IntegrationTests.Clock;

public partial class FakeClock
{
    private class DelayEvent : BaseDelayEvent, IEvent
    {
        public DateTime TriggerTime { get; }

        public DelayEvent(DateTime triggerTime, AsyncTrackingSyncContext asyncTestSyncContext, CancellationToken cancellationToken)
            : base(asyncTestSyncContext, cancellationToken)
        {
            TriggerTime = triggerTime;
        }
    }
}