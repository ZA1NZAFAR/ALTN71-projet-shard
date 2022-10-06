using System.Security;

namespace Shard.Shared.Web.IntegrationTests.Clock.TaskTracking;

/// <summary>
/// XUnit tracks asynchronous tasks through AsyncTestSyncContext.
/// This is good enough to track async void tests.
/// 
/// However that SynchronizationContext does not force the callback to be executed on itself.
/// The problem is, MaxConcurrencySyncContext do so. See <see cref="AsyncTestSyncContext.RunOnSyncContext"/>
/// Therefore, any XUnit thread from its own custom thread pool (XunitWorkerThread) 
/// running a Task will override its captured SynchronizationContext by a MaxConcurrencySyncContext. 
/// 
/// Should those tasks themselves post new job on the pool (for example by completing a task another one 
/// is waiting on), those jobs will not be tracked.
/// As a result, AsyncTestSyncContext is unsuitable to track cascading tasks posting.
/// Besides, that synchronization context cannot be nested in another instance of itself, and it only is 
/// installed before running the test, instead of before instanciating the test class.
/// 
/// This class aims to track all outstanding tasks so that our FakeClock can wait for them.
/// It is inspired from AsyncTestSyncContext (for the most part) and MaxConcurrencySyncContext (for the 
/// synchronization context overriding), is simplified so that it immediatly wraps the current synchronization
/// context, do net care about returned exception, only works when there is one, and play well with inner 
/// synchronization contexts.
/// 
/// Sources :
/// https://github.com/xunit/xunit/blob/master/src/xunit.execution/Sdk/AsyncTestSyncContext.cs
/// https://github.com/xunit/xunit/blob/master/src/xunit.execution/Sdk/MaxConcurrencySyncContext.cs
/// </summary>
public class AsyncTrackingSyncContext : SynchronizationContext
{
    public static AsyncTrackingSyncContext Setup()
    {
        var synchronizationContext = new AsyncTrackingSyncContext();
        synchronizationContext.SetCurrentAsSynchronizationContext();
        return synchronizationContext;
    }

    [SecuritySafeCritical]
    void SetCurrentAsSynchronizationContext() => SetSynchronizationContext(this);


    readonly AsyncManualResetEvent @event = new(true);
    readonly SynchronizationContext innerContext;
    int operationCount;

    private AsyncTrackingSyncContext()
    {
        innerContext = Current ?? new SynchronizationContext();
    }

    public override void OperationCompleted()
    {
        var result = Interlocked.Decrement(ref operationCount);
        if (result == 0)
            @event.Set();
        innerContext.OperationCompleted();
    }

    public override void OperationStarted()
    {
        Interlocked.Increment(ref operationCount);
        @event.Reset();
        innerContext.OperationStarted();
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        // The call to Post() may be the state machine signaling that an exception is
        // about to be thrown, so we make sure the operation count gets incremented
        // before the Task.Run, and then decrement the count when the operation is done.
        OperationStarted();

        try
        {
            innerContext.Post(_ =>
            {
                try
                {
                    RunOnSyncContext(d, state);
                }
                catch { }
                finally
                {
                    OperationCompleted();
                }
            }, null);
        }
        catch { }
    }

    public override void Send(SendOrPostCallback d, object? state)
    {
        try
        {
            innerContext.Send(_ => RunOnSyncContext(d, state), null);
        }
        catch { }
    }

    [SecuritySafeCritical]
    void RunOnSyncContext(SendOrPostCallback callback, object? state)
    {
        var oldSyncContext = Current;
        SetSynchronizationContext(this);
        callback(state);
        SetSynchronizationContext(oldSyncContext);
    }

    /// <summary>
    /// Returns a task which is signaled when all outstanding operations are complete.
    /// </summary>
    public async Task WaitForCompletionAsync() => await @event.WaitAsync();
}
