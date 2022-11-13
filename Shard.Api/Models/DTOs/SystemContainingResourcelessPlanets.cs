using Shard.Api.Models.EditableObjects;
using Shard.Shared.Core;

namespace Shard.Api.Models;

public class SystemContainingResourcelessPlanets
{
    public string Name { get; }
    public List<ResourcelessPlanet> Planets { get; }

    public SystemContainingResourcelessPlanets(SystemSpecificationEditable systemSpecification)
    {
        Name = systemSpecification.Name;
        Planets = systemSpecification.Planets.Select(p => new ResourcelessPlanet(p)).ToList();
    }

    public SystemContainingResourcelessPlanets(string name, List<ResourcelessPlanet> planets)
    {
        Name = name;
        Planets = planets;
    }
}