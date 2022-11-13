using Shard.Api.Tools;
using Shard.Shared.Core;

namespace Shard.Api.Services;

public interface ICelestialService
{
    public IReadOnlyList<SystemSpecificationEditable> GetAllSystemsAndPlanets();
    SystemSpecificationEditable GetSystemAndPlanets(string systemName);
    IReadOnlyList<PlanetSpecificationEditable> GetPlanetsOfSystem(string systemName);
    PlanetSpecificationEditable GetPlanetOfSystem(string systemName, string planetName);
    SystemSpecificationEditable GetRandomSystem();
}

public class CelestialService : ICelestialService
{
    private readonly SectorSpecificationEditable _universe;

    public CelestialService()
    {
        MapGeneratorOptions mapGeneratorOptions = new MapGeneratorOptions();
        mapGeneratorOptions.Seed = "Test application";
        var mapGenerator = new MapGenerator(mapGeneratorOptions);
        _universe = SwissKnife.SectorToEditableSector(mapGenerator.Generate());
    }

    public IReadOnlyList<SystemSpecificationEditable> GetAllSystemsAndPlanets()
    {
        var sys = _universe.Systems;
        return sys;
    }

    public SystemSpecificationEditable GetSystemAndPlanets(string systemName)
    {
        var sys = _universe.Systems;
        return sys.FirstOrDefault(x => x.Name == systemName);
    }

    public IReadOnlyList<PlanetSpecificationEditable> GetPlanetsOfSystem(string systemName)
    {
        var sys = _universe.Systems;
        var system = sys.FirstOrDefault(x => x.Name == systemName);
        return system.Planets;
    }

    public PlanetSpecificationEditable GetPlanetOfSystem(string systemName, string planetName)
    {
        var sys = _universe.Systems;
        var system = sys.FirstOrDefault(x => x.Name == systemName);
        var planet = system.Planets.FirstOrDefault(x => x.Name == planetName);
        return planet;
    }

    public SystemSpecificationEditable GetRandomSystem()
    {
        var sys = _universe.Systems;
        var system = sys.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
        return system;
    }
}