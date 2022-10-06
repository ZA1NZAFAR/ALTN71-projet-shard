using Shard.Shared.Web.IntegrationTests.Clock.TaskTracking;
namespace Shard.Shared.Web.IntegrationTests.Clock;

public partial class FakeClock
{
    private class BaseDelayEvent
    {
        private readonly TaskCompletionSource<object> taskCompletionSource = new();
        private readonly AsyncTrackingSyncContext asyncTestSyncContext;

        public Task Task => taskCompletionSource.Task;

        public BaseDelayEvent(AsyncTrackingSyncContext asyncTestSyncContext, CancellationToken cancellationToken)
        {
            // If the current delay is being created from within a Task.Run or any other thread that did not inherit 
            // the synchronization context, ensure the the await that will happen will capture the asyncTestSyncContext

            if (SynchronizationContext.Current == null)
                SynchronizationContext.SetSynchronizationContext(asyncTestSyncContext);

            cancellationToken.Register(
                () => taskCompletionSource.TrySetCanceled(cancellationToken));

            this.asyncTestSyncContext = asyncTestSyncContext;
        }

        public async Task TriggerAsync()
        {
            taskCompletionSource.SetResult(this);

            /* We want to ensure all tasks ready to start are triggered before we move on
             * 
             * The reasoning was to allow to any pending thread a reasonable amount of time to react to the
             * task being completed. However in some cases, some handler blocks (often because of an error 
             * or a race condition).
             * 
             * Instead of breaking every tests (and consuming all CI credits), we allow a maximum of two
             * seconds before moving forward.
             */

            await Task.WhenAny(
                asyncTestSyncContext.WaitForCompletionAsync(),
                Task.Delay(2000));
        }
    }
}
