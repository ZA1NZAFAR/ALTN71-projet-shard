using System.Collections.ObjectModel;
using Shard.Shared.Core;

namespace Shard.Api.Tools;

public class SwissKnife
{
    public static ResourceKind getResourceKindFromString(string resource)
    {
        foreach (ResourceKind resourceKind in Enum.GetValues(typeof(ResourceKind)))
        {
            if (resourceKind.ToString().ToLower() == resource.ToLower())
            {
                return resourceKind;
            }
        }
        throw new Exception("Resource not found");
    }
    
    public static ResourceKind getHighestResource(PlanetSpecificationEditable planet)
    {
        var resources = planet.ResourceQuantity;
        var highestResource = resources.First().Key;
        var highestQuantity = 0;

        foreach (var resource in resources)
        {
            if (resources[resource.Key] > highestQuantity)
            {
                highestQuantity = resources[resource.Key];
                highestResource = resource.Key;
            }
        }
        return highestResource;
    }
    
    public static SectorSpecificationEditable sectorToEditableSector(SectorSpecification sector)
    {
        List<SystemSpecificationEditable> systems = sector.Systems.Select(system => systemToEditableSystem(system)).ToList();
        return new SectorSpecificationEditable(systems);
    }

    private static SystemSpecificationEditable systemToEditableSystem(SystemSpecification system)
    {
        List<PlanetSpecificationEditable> planets = system.Planets.Select(planet => planetToEditablePlanet(planet)).ToList();
        return new SystemSpecificationEditable(system.Name, planets);
    }

    private static PlanetSpecificationEditable planetToEditablePlanet(PlanetSpecification planet)
    {
        return new PlanetSpecificationEditable(planet.Name,planet.Size, planet.ResourceQuantity.ToDictionary(x => x.Key, x => x.Value));
    }
}
