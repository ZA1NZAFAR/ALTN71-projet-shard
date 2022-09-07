namespace Shard.Shared.Core;

public class PlanetSpecification
{
    public string Name { get; }
    public int Size { get; }
    
    public IReadOnlyDictionary<ResourceKind, int> ResourceQuantity { get; }

    internal PlanetSpecification(Random random)
    {
        Name = random.NextGuid().ToString();
                   
        Size = 1 + random.Next(999);
        ResourceQuantity = new RandomShareComputer(random).GenerateResources(Size);
    }
}
