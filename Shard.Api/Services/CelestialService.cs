using Shard.Shared.Core;

namespace Shard.Api.Services;

public interface ICelestialService
{
    public IReadOnlyList<SystemSpecification> getAllSystemsAndPlanets();
    SystemSpecification getSystemAndPlanets(string systemName);
    IReadOnlyList<PlanetSpecification> getPlanetsOfSystem(string systemName);
    PlanetSpecification getPlanetOfSystem(string systemName, string planetName);
}

public class CelestialService : ICelestialService
{
    static MapGenerator _mapGenerator;
    static SectorSpecification _universe;

    public CelestialService()
    {
        MapGeneratorOptions mapGeneratorOptions = new MapGeneratorOptions();
        mapGeneratorOptions.Seed = "TheUltimateSeed";
        _mapGenerator = new MapGenerator(mapGeneratorOptions);
        _universe = _mapGenerator.Generate();
    }

    public IReadOnlyList<SystemSpecification> getAllSystemsAndPlanets()
    {
        var sys = _universe.Systems;
        return sys;
    }

    public SystemSpecification getSystemAndPlanets(string systemName)
    {
        var sys = _universe.Systems;
        return sys.FirstOrDefault(x => x.Name == systemName);
    }

    public IReadOnlyList<PlanetSpecification> getPlanetsOfSystem(string systemName)
    {
        var sys = _universe.Systems;
        var system = sys.FirstOrDefault(x => x.Name == systemName);
        return system.Planets;
    }

    public PlanetSpecification getPlanetOfSystem(string systemName, string planetName)
    {
        var sys = _universe.Systems;
        var system = sys.FirstOrDefault(x => x.Name == systemName);
        var planet = system.Planets.FirstOrDefault(x => x.Name == planetName);
        return planet;
    }
}