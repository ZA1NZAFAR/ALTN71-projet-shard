using Shard.Shared.Core;

namespace Shard.Api.Models;

public class SystemContainingResourcelessPlanets
{
    public string Name { get; }
    public List<ResourcelessPlanet> Planets { get; }
    
    public SystemContainingResourcelessPlanets(SystemSpecification systemSpecification)
    {
        Name = systemSpecification.Name;
        Planets = systemSpecification.Planets.Select(p => new ResourcelessPlanet(p)).ToList();
    }

    public SystemContainingResourcelessPlanets(string name, List<ResourcelessPlanet> planets)
    {
        Name = name;
        Planets = planets;
    }
    
    public void AddPlanet(ResourcelessPlanet planet)
    {
        Planets.Add(planet);
    }
}