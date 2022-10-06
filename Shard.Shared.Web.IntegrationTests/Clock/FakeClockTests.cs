namespace Shard.Shared.Web.IntegrationTests.Clock;

public class FakeClockTests
{
    private readonly FakeClock clock = new();

    [Fact]
    public async Task SetNow_RetainsValue()
    {
        var value = new DateTime(2019, 10, 03, 08, 00, 00);
        await clock.SetNow(value);
        Assert.Equal(value, clock.Now);
    }

    [Fact]
    public async Task Delay_ThrowsIfBellowMinusOne()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            "delay",
            () => clock.Delay(-2));
    }

    [Fact]
    public async Task Delay_ThrowsIfHighestThanMaxValue()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            "delay",
            () => clock.Delay(TimeSpan.MaxValue));
    }

    [Fact]
    public void Delay_MinusOne_ReturnsPendingTask()
    {
        var task = clock.Delay(-1);
        Assert.Equal(TaskStatus.WaitingForActivation, task.Status);
    }

    [Fact]
    public async Task Delay_MinusOne_InfiniteWait()
    {
        var task = clock.Delay(-1);
        await clock.SetNow(DateTime.MaxValue);
        Assert.Equal(TaskStatus.WaitingForActivation, task.Status);
    }

    [Fact]
    public void Delay_MinusOne_CanBeCancelled()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        var task = clock.Delay(-1, cancellationTokenSource.Token);
        cancellationTokenSource.Cancel();

        Assert.Equal(TaskStatus.Canceled, task.Status);
    }

    [Fact]
    public void Delay_AlreadyCancelled_ReturnsCanceledTask()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        
        cancellationTokenSource.Cancel();
        var task = clock.Delay(0, cancellationTokenSource.Token);

        Assert.Equal(TaskStatus.Canceled, task.Status);
    }

    [Fact]
    public void Delay_Instant_ReturnsCompletedTask()
    {
        var task = clock.Delay(0);
        Assert.Equal(Task.Delay(0).Status, task.Status);
        Assert.Equal(TaskStatus.RanToCompletion, task.Status);
    }

    [Fact]
    public void Delay_5Sec_ReturnsPendingTask()
    {
        var task = clock.Delay(5000);
        Assert.Equal(TaskStatus.WaitingForActivation, task.Status);
    }

    [Fact]
    public void Delay_5Sec_CanBeCancelled()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        var task = clock.Delay(5000, cancellationTokenSource.Token);
        cancellationTokenSource.Cancel();

        Assert.Equal(TaskStatus.Canceled, task.Status);
    }

    [Fact]
    public async Task Delay_5Sec_NotRanJustBefore()
    {
        var task = clock.Delay(5000);
        await clock.SetNow(clock.Now.AddMilliseconds(5000 - 1));
        Assert.Equal(TaskStatus.WaitingForActivation, task.Status);
    }

    [Fact]
    public async Task Delay_5Sec_RanWhenReachingTime()
    {
        var task = clock.Delay(5000);
        await clock.SetNow(clock.Now.AddMilliseconds(5000));
        Assert.Equal(TaskStatus.RanToCompletion, task.Status);
    }

    [Fact]
    public async Task Delay_5Sec_RanWhenReachingTime_EvenWithMultiplePushes()
    {
        var task = clock.Delay(5000);
        await clock.SetNow(clock.Now.AddMilliseconds(1000));
        await clock.SetNow(clock.Now.AddMilliseconds(2000));
        await clock.SetNow(clock.Now.AddMilliseconds(1000));
        await clock.SetNow(clock.Now.AddMilliseconds(1000));
        Assert.Equal(TaskStatus.RanToCompletion, task.Status);
    }

    [Fact]
    public async Task Delay_5Sec_RanRightOnTime()
    {
        await clock.SetNow(new DateTime(2019, 10, 03, 08, 00, 00));

#pragma warning disable IDE0059 // Faux positif
        DateTime? triggerTime = null;
#pragma warning restore IDE0059 // Faux positif

        var task = TestMethod();

        await clock.SetNow(clock.Now.AddMilliseconds(1000));
        await clock.SetNow(clock.Now.AddMilliseconds(2000));
        await clock.SetNow(clock.Now.AddMilliseconds(1000));
        await clock.SetNow(clock.Now.AddMilliseconds(2000));
        await task;
        Assert.Equal(new DateTime(2019, 10, 03, 08, 00, 05), triggerTime);

        async Task TestMethod()
        {
            await clock.Delay(5000);
            triggerTime = clock.Now;
        }
    }

    [Fact]
    public async Task CreateTimer_InfiniteDueTime_NeverTriggers()
    {
        bool triggered = false;
        using var timer = clock.CreateTimer(
            _ => triggered = true,
            state: null,
            dueTime: -1,
            period: 1);

        await clock.SetNow(clock.Now.AddDays(30));
        Assert.False(triggered);
    }

    [Fact]
    public void CreateTimer_ZeroDueTime_TriggersImmediatly()
    {
        bool triggered = false;
        using var timer = clock.CreateTimer(
            _ => triggered = true,
            state: null,
            dueTime: 0,
            period: 1);

        Assert.True(triggered);
    }

    [Fact]
    public async Task CreateTimer_5msDueTime_NotTriggeredAt4ms()
    {
        bool triggered = false;
        using var timer = clock.CreateTimer(
            _ => triggered = true,
            state: null,
            dueTime: 5,
            period: 1);

        await clock.SetNow(clock.Now.AddMilliseconds(4));
        Assert.False(triggered);
    }

    [Fact]
    public async Task CreateTimer_5msDueTime_TriggeredAt5ms()
    {
        bool triggered = false;
        using var timer = clock.CreateTimer(
            _ => triggered = true,
            state: null,
            dueTime: 5,
            period: 1);

        await clock.SetNow(clock.Now.AddMilliseconds(5));
        Assert.True(triggered);
    }

    [Fact]
    public async Task CreateTimer_5msDueTime_And0Period_TriggeredOnce()
    {
        bool triggered = false;
        using var timer = clock.CreateTimer(
            _ => triggered = true,
            state: null,
            dueTime: 5,
            period: 0);

        await clock.SetNow(clock.Now.AddMilliseconds(5));

        triggered = false;
        await clock.SetNow(clock.Now.AddDays(30));
        Assert.False(triggered);
    }

    [Fact]
    public async Task CreateTimer_5msDueTime_AndInfinitePeriod_TriggeredOnce()
    {
        bool triggered = false;
        using var timer = clock.CreateTimer(
            _ => triggered = true,
            state: null,
            dueTime: 5,
            period: -1);

        await clock.SetNow(clock.Now.AddMilliseconds(5));

        triggered = false;
        await clock.SetNow(clock.Now.AddDays(30));
        Assert.False(triggered);
    }

    [Fact]
    public async Task CreateTimer_5msDueTime_And2msPeriod_TriggeredOnceAt6ms()
    {
        bool triggered = false;
        using var timer = clock.CreateTimer(
            _ => triggered = true,
            state: null,
            dueTime: 5,
            period: 2);

        await clock.SetNow(clock.Now.AddMilliseconds(5));

        triggered = false;
        await clock.SetNow(clock.Now.AddMilliseconds(1));
        Assert.False(triggered);
    }

    [Fact]
    public async Task CreateTimer_5msDueTime_And2msPeriod_TriggeredTwiceAt7ms()
    {
        bool triggered = false;
        using var timer = clock.CreateTimer(
            _ => triggered = true,
            state: null,
            dueTime: 5,
            period: 2);

        await clock.SetNow(clock.Now.AddMilliseconds(5));

        triggered = false;
        await clock.SetNow(clock.Now.AddMilliseconds(2));
        Assert.True(triggered);
    }

    [Fact]
    public async Task CreateTimer_5msDueTime_And2msPeriod_TriggeredTwiceAt7ms_EvenAtOnce()
    {
        int triggeredCount = 0;
        using var timer = clock.CreateTimer(
            _ => triggeredCount++,
            state: null,
            dueTime: 5,
            period: 2);

        await clock.SetNow(clock.Now.AddMilliseconds(7));
        Assert.Equal(2, triggeredCount);
    }

    [Fact]
    public async Task CreateTimer_5msDueTime_And2msPeriod_TriggeredThriceAt9ms_EvenAtOnce()
    {
        int triggeredCount = 0;
        using var timer = clock.CreateTimer(
            _ => triggeredCount++,
            state: null,
            dueTime: 5,
            period: 2);

        await clock.SetNow(clock.Now.AddMilliseconds(9));
        Assert.Equal(3, triggeredCount);
    }

    [Fact]
    public async Task CreateTimer_StateIsPassedWhenReachingTime()
    {
        object expectedState = new();
        object? triggeredState = null;
        using var timer = clock.CreateTimer(
            state => triggeredState = state,
            state: expectedState,
            dueTime: 5,
            period: 2);

        await clock.SetNow(clock.Now.AddMilliseconds(5));
        Assert.Same(expectedState, triggeredState);
    }

    [Fact]
    public async Task TimerChange_ResetTimer()
    {
        await clock.SetNow(new DateTime(2019, 10, 03, 08, 00, 00, 000));

        int triggeredCount = 0;
        using var timer = clock.CreateTimer(
            _ => triggeredCount++,
            state: null,
            dueTime: 5,
            period: 2);

        await clock.SetNow(new DateTime(2019, 10, 03, 08, 00, 00, 004));
        Assert.Equal(0, triggeredCount);

        timer.Change(2, 4);

        await clock.SetNow(new DateTime(2019, 10, 03, 08, 00, 00, 005));
        Assert.Equal(0, triggeredCount);

        await clock.SetNow(new DateTime(2019, 10, 03, 08, 00, 00, 006));
        Assert.Equal(1, triggeredCount);

        await clock.SetNow(new DateTime(2019, 10, 03, 08, 00, 00, 007));
        Assert.Equal(1, triggeredCount);

        await clock.SetNow(new DateTime(2019, 10, 03, 08, 00, 00, 008));
        Assert.Equal(1, triggeredCount);

        await clock.SetNow(new DateTime(2019, 10, 03, 08, 00, 00, 009));
        Assert.Equal(1, triggeredCount);

        await clock.SetNow(new DateTime(2019, 10, 03, 08, 00, 00, 010));
        Assert.Equal(2, triggeredCount);

        await clock.SetNow(new DateTime(2019, 10, 03, 08, 00, 00, 013));
        Assert.Equal(2, triggeredCount);

        await clock.SetNow(new DateTime(2019, 10, 03, 08, 00, 00, 014));
        Assert.Equal(3, triggeredCount);
    }

}
