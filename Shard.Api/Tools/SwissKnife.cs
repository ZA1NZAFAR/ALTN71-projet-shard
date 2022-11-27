using Shard.Api.Models;
using Shard.Api.Models.EditableObjects;
using Shard.Api.Services;
using Shard.Shared.Core;

namespace Shard.Api.Tools;

// Contains various helper methods for working with the Shard API.
public static class SwissKnife
{
    // Get the highest quantity solid resource
    public static ResourceKind GetHighestResource(PlanetSpecificationEditable planet)
    {
        var resources = planet.ResourceQuantity;
        var highestResource = resources.First().Key;
        var highestQuantity = 0;

        foreach (var resource in resources)
        {
            if (resources[resource.Key] > highestQuantity && resource.Key != ResourceKind.Water &&
                resource.Key != ResourceKind.Oxygen)
            {
                highestQuantity = resources[resource.Key];
                highestResource = resource.Key;
            }
            else if (resources[resource.Key] == highestQuantity)
            {
                highestResource = GetMoreImportantResource(highestResource, resource.Key);
            }
        }

        return highestResource;
    }

    // Get the more important resource between two resources
    private static ResourceKind GetMoreImportantResource(ResourceKind resource1, ResourceKind resource2)
    {
        var priority = new List<ResourceKind>
        {
            ResourceKind.Titanium,
            ResourceKind.Gold,
            ResourceKind.Aluminium,
            ResourceKind.Iron,
            ResourceKind.Carbon
        };

        var resource1Index = priority.IndexOf(resource1);
        var resource2Index = priority.IndexOf(resource2);
        return resource1Index < resource2Index ? resource1 : resource2;
    }

    // To convert a generated Sector to an editable one
    public static SectorSpecificationEditable SectorToEditableSector(SectorSpecification sector)
    {
        List<SystemSpecificationEditable> systems =
            sector.Systems.Select(SystemToEditableSystem).ToList();
        return new SectorSpecificationEditable(systems);
    }

    // To convert a generated System to an editable one
    private static SystemSpecificationEditable SystemToEditableSystem(SystemSpecification system)
    {
        List<PlanetSpecificationEditable> planets =
            system.Planets.Select(PlanetToEditablePlanet).ToList();
        return new SystemSpecificationEditable(system.Name, planets);
    }

    // To convert a generated Planet to an editable one
    private static PlanetSpecificationEditable PlanetToEditablePlanet(PlanetSpecification planet)
    {
        return new PlanetSpecificationEditable(planet.Name, planet.Size,
            planet.ResourceQuantity.ToDictionary(x => x.Key, x => x.Value));
    }


    // Updates the resources of the user according to the time passed and the machines created
    public static void UpdateResources(User res, IUserService _userService, ICelestialService _celestialService,
        IClock _clock)
    {
        List<Building> buildings;
        try
        {
            buildings = _userService.GetBuildingsOfUserById(res.Id);
        }
        catch (Exception)
        {
            return;
        }

        foreach (var building in buildings)
        {
            // if the building is not finished, we don't update the resources
            if (!building.IsBuilt)
            {
                continue;
            }

            // 1 minute = 1 resource
            var minutes = (int)(_clock.Now - building.LastUpdate).TotalMinutes;
            // planet of the machine
            var planet = _celestialService.GetPlanetOfSystem(building.System, building.Planet);
            ResourceKind resource;

            if (building.ResourceCategory == "gaseous")
            {
                resource = ResourceKind.Oxygen;
            }
            else if (building.ResourceCategory == "liquid")
            {
                resource = ResourceKind.Water;
            }
            else
            {
                // if solid then extract resources based on the quantity and priority
                resource = SwissKnife.GetHighestResource(planet);
            }

            // keep extracting resources until the planet is empty or the time is up
            while (minutes > 0 && planet.ResourceQuantity[resource] > 0)
            {
                res.ResourcesQuantity[resource]++;
                planet.ResourceQuantity[resource]--;
                minutes--;

                // if the resource depleted, get the next one
                if (minutes > 0 && planet.ResourceQuantity[resource] == 0 && !IsExhausted(planet) &&
                    building.ResourceCategory == "solid")
                {
                    resource = SwissKnife.GetHighestResource(planet);
                }
            }

            building.LastUpdate = _clock.Now;
        }
    }

    // check if the planet resources are exhausted
    private static bool IsExhausted(PlanetSpecificationEditable planet)
    {
        return planet.ResourceQuantity.All(resource => resource.Value <= 0);
    }

    // check and remove the under construction buildings when the builder leaves the planet/system
    public static void CheckAndRemoveOngoingBuildingsIfChangePlanet(string userId, string unitId,
        Dictionary<User, List<Unit>> usersUnitsDb, Dictionary<User, List<Building>> usersBuildingsDb)
    {
        var user = usersUnitsDb.Keys.First(u => u.Id == userId);
        var unit = usersUnitsDb[user].FirstOrDefault(u => u.Id == unitId);
        if (usersBuildingsDb.ContainsKey(user))
        {
            var buildingsToRemove = usersBuildingsDb[user].Where(b => b.BuilderId == unitId).ToList();
            foreach (var building in buildingsToRemove.Where(building =>
                         (unit!.Planet != unit.DestinationPlanet && unit.Planet == building.Planet &&
                          !building.IsBuilt) ||
                         (unit.System != unit.DestinationSystem && unit.System == building.System &&
                          !building.IsBuilt)))
            {
                usersBuildingsDb[user].Remove(building);
            }
        }
    }

    public static Dictionary<ResourceKind, int> GetUnitCost(string unitType)
    {
        switch (unitType)
        {
            case "scout": return new Dictionary<ResourceKind, int>
            {
                { ResourceKind.Carbon, 5 },
                { ResourceKind.Iron, 5 }
            };
            case "builder": return new Dictionary<ResourceKind, int>
            {
                { ResourceKind.Carbon, 5 },
                { ResourceKind.Iron, 10 }
            };
            case "fighter": return new Dictionary<ResourceKind, int>
            {
                { ResourceKind.Iron, 20 },
                { ResourceKind.Aluminium, 10 }
            };
            case "bomber": return new Dictionary<ResourceKind, int>
            {
                { ResourceKind.Iron, 20 },
                { ResourceKind.Titanium, 10 }
            };
            case "cruiser": return new Dictionary<ResourceKind, int>
            {
                { ResourceKind.Iron, 60 },
                { ResourceKind.Gold, 20 }
            };
            default: throw new Exception("Invalid unit type");
        }
    }
    
    //check if unit is a combat unit
    public static bool IsCombatUnit(string unitType)
    {
        return unitType == "fighter" || unitType == "bomber" || unitType == "cruiser";
    }
}