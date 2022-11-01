using Shard.Shared.Core;

namespace Shard.Api.Services;

public interface ICelestialService
{
    public IReadOnlyList<SystemSpecification> GetAllSystemsAndPlanets();
    SystemSpecification GetSystemAndPlanets(string systemName);
    IReadOnlyList<PlanetSpecification> GetPlanetsOfSystem(string systemName);
    PlanetSpecification GetPlanetOfSystem(string systemName, string planetName);
    SystemSpecification GetRandomSystem();
}

public class CelestialService : ICelestialService
{
    private MapGenerator _mapGenerator;
    private SectorSpecification _universe;

    public CelestialService()
    {
        MapGeneratorOptions mapGeneratorOptions = new MapGeneratorOptions();
        mapGeneratorOptions.Seed = "TheUltimateSeed";
        _mapGenerator = new MapGenerator(mapGeneratorOptions);
        _universe = _mapGenerator.Generate();
    }

    public IReadOnlyList<SystemSpecification> GetAllSystemsAndPlanets()
    {
        var sys = _universe.Systems;
        return sys;
    }

    public SystemSpecification GetSystemAndPlanets(string systemName)
    {
        var sys = _universe.Systems;
        return sys.FirstOrDefault(x => x.Name == systemName);
    }

    public IReadOnlyList<PlanetSpecification> GetPlanetsOfSystem(string systemName)
    {
        var sys = _universe.Systems;
        var system = sys.FirstOrDefault(x => x.Name == systemName);
        return system.Planets;
    }

    public PlanetSpecification GetPlanetOfSystem(string systemName, string planetName)
    {
        var sys = _universe.Systems;
        var system = sys.FirstOrDefault(x => x.Name == systemName);
        var planet = system.Planets.FirstOrDefault(x => x.Name == planetName);
        return planet;
    }

    public SystemSpecification GetRandomSystem()
    {
        var sys = _universe.Systems;
        var system = sys.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
        return system;
    }
}