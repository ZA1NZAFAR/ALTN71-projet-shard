namespace Shard.Shared.Core;

public class PlanetSpecification
{
    public string Name { get; }
    public int Size { get; }
    
    internal PlanetSpecification(Random random)
    {
        Name = random.NextGuid().ToString();
                   
        Size = 1 + random.Next(999);
    }
}
