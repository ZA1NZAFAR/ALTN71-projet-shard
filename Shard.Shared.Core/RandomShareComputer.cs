namespace Shard.Shared.Core;

public partial class RandomShareComputer
{
    private interface IResource
    {
        ResourceKind Kind { get; }
        double ProbabilityOfPresence { get; }
        int PresenceMultiplier { get; }
    }

    private static readonly IResource[] solidResources = new IResource[]
    {
        new Carbon(),
        new Iron(),
        new Gold(),
        new Aluminium(),
        new Titanium(),
    };

    private static readonly IResource[] liquidResources = new IResource[]
    {
        new Water(),
    };

    private static readonly IResource[] gaseousResources = new IResource[]
    {
        new Oxygen(),
    };

    private readonly Random random;

    public RandomShareComputer(Random random)
    {
        this.random = random;
    }

    public IReadOnlyDictionary<ResourceKind, int> GenerateResources(int size)
    {
        var entries = GenerateResources(size, solidResources)
            .Concat(GenerateResources(size, liquidResources))
            .Concat(GenerateResources(size, gaseousResources));

        return new Dictionary<ResourceKind, int>(entries);
    }

    private IEnumerable<KeyValuePair<ResourceKind, int>> GenerateResources(int size, IResource[] resources)
    {
        checked
        {
            var presentResources = resources
                .Where(resource => random.NextDouble() <= resource.ProbabilityOfPresence);

            var resourcePresence = presentResources
                .Select(resource => new
                {
                    Resource = resource,
                    Presence = random.NextDouble() * resource.PresenceMultiplier
                })
                .ToList();

            var totalPresence = resourcePresence.Sum(resource => resource.Presence);
            var sizeAllocatedToResources = random.NextDouble() * size;

            return resourcePresence.Select(resource => KeyValuePair.Create(
                resource.Resource.Kind,
                (int)Math.Round(resource.Presence / totalPresence * sizeAllocatedToResources)));
        }
    }
}
