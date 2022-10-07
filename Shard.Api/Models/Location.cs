using Shard.Shared.Core;

namespace Shard.Api.Models;

public class Location
{
    public string system { get; set; }
    public string planet { get; set; }
    public Dictionary<string, int> resourcesQuantity { get; }

    public Location(string syste, PlanetSpecification planetName)
    {
        system = syste;
        planet = planetName.Name;
        resourcesQuantity = new Dictionary<string, int>();
        var dictionary = new Dictionary<string, int>();
        foreach (KeyValuePair<string, int> entry in resourcesQuantity)
        {
            resourcesQuantity.Add(entry.Key.ToLower(), entry.Value);
        }
    }
}