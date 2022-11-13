using Shard.Api.Models.EditableObjects;

namespace Shard.Api.Models;

public class Location
{
    public string System { get; }
    public string Planet { get; }
    public Dictionary<string, int> ResourcesQuantity { get; set; }

    public Location(string system, PlanetSpecificationEditable planetName)
    {
        System = system;
        Planet = planetName.Name;
        ResourcesQuantity = new Dictionary<string, int>();
        foreach (var entry in ResourcesQuantity)
            ResourcesQuantity.Add(entry.Key.ToLower(), entry.Value);
    }
}