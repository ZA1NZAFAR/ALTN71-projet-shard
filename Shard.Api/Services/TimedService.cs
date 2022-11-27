using Shard.Api.Tools;
using Shard.Shared.Core;

namespace Shard.Api.Services;

public class TimedService : IHostedService, IDisposable
{
    private ITimer? _timer = null;
    private readonly IClock _clock;
    private readonly IUserService _userService;
    private readonly ICelestialService _celestialService;

    public TimedService(IClock clock, IUserService userService, ICelestialService celestialService)
    {
        _clock = clock;
        _userService = userService;
        _celestialService = celestialService;
    }


    public Task StartAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Timed Hosted Service running.");

        _timer = _clock.CreateTimer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
         BackGroundTasks.Fight(_userService, _celestialService, _clock);
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Timed Hosted Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}