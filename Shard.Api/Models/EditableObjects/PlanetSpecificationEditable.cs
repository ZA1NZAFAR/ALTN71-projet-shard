namespace Shard.Shared.Core;

public class PlanetSpecificationEditable
{
    public string Name { get; }
    public int Size { get; }
    
    public Dictionary<ResourceKind, int> ResourceQuantity { get; }

    internal PlanetSpecificationEditable(string name,int size, Dictionary<ResourceKind, int> resourceQuantity)
    {
        Name = name;
        Size = size;
        ResourceQuantity = resourceQuantity;
    }
}
