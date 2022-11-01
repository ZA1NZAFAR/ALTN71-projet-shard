using Shard.Shared.Core;

namespace Shard.Api.Helpers;

public class SwissKnife
{
    public static async Task WaitAsync(IClock clock, int waitingTime)
    {
        await Task.Run(() => clock.Delay(waitingTime));
    }
}