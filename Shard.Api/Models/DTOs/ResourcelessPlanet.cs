using Shard.Shared.Core;

namespace Shard.Api.Models;

public class ResourcelessPlanet
{
    public string Name { get; }
    public int Size { get; }
    
    
    public ResourcelessPlanet(PlanetSpecificationEditable planetSpecification)
    {
        Name = planetSpecification.Name;
        Size = planetSpecification.Size;
    }

    public ResourcelessPlanet(string name, int size)
    {
        Name = name;
        Size = size;
    }
}