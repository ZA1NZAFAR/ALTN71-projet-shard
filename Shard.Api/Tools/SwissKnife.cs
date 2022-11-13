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
            else if (resources[resource.Key] == highestQuantity)
            {
                highestResource = getMoreImportantRessorce(highestResource, resource.Key);
            }
        }

        return highestResource;
    }

    private static ResourceKind getMoreImportantRessorce(ResourceKind resorce1, ResourceKind resource2)
    {
        var priority = new List<ResourceKind>
        {
            ResourceKind.Titanium,
            ResourceKind.Gold,
            ResourceKind.Aluminium,
            ResourceKind.Iron,
            ResourceKind.Carbon
        };

        var resorce1Index = priority.IndexOf(resorce1);
        var resource2Index = priority.IndexOf(resource2);
        if (resorce1Index < resource2Index)
        {
            return resorce1;
        }

        return resource2;
    }

    public static SectorSpecificationEditable sectorToEditableSector(SectorSpecification sector)
    {
        List<SystemSpecificationEditable> systems =
            sector.Systems.Select(system => systemToEditableSystem(system)).ToList();
        return new SectorSpecificationEditable(systems);
    }

    private static SystemSpecificationEditable systemToEditableSystem(SystemSpecification system)
    {
        List<PlanetSpecificationEditable> planets =
            system.Planets.Select(planet => planetToEditablePlanet(planet)).ToList();
        return new SystemSpecificationEditable(system.Name, planets);
    }

    private static PlanetSpecificationEditable planetToEditablePlanet(PlanetSpecification planet)
    {
        return new PlanetSpecificationEditable(planet.Name, planet.Size,
            planet.ResourceQuantity.ToDictionary(x => x.Key, x => x.Value));
    }
}