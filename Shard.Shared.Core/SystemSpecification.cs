namespace Shard.Shared.Core;

public class SystemSpecification
{
    public string Name { get; }
    public IReadOnlyList<PlanetSpecification> Planets { get; }

    internal SystemSpecification(Random random)
    {
        Name = random.NextGuid().ToString();

        var planetCount = 1 + random.Next(9);
        Planets = Generate(planetCount, random);
    }

    private static List<PlanetSpecification> Generate(int count, Random random)
    {
        return Enumerable.Range(1, count)
            .Select(_ => new PlanetSpecification(random))
            .ToList();
    }
}
