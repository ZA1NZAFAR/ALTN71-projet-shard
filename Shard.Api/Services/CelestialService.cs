using Shard.Api.Models.EditableObjects;
using Shard.Api.Tools;
using Shard.Shared.Core;

namespace Shard.Api.Services;

public interface ICelestialService
{
    public List<SystemSpecificationEditable> GetAllSystemsAndPlanets();
    SystemSpecificationEditable GetSystemAndPlanets(string systemName);
    List<PlanetSpecificationEditable> GetPlanetsOfSystem(string systemName);
    PlanetSpecificationEditable GetPlanetOfSystem(string systemName, string planetName);
    SystemSpecificationEditable GetRandomSystem();
}

// Service that stores the celestial objects and returns them when requested
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

    public List<SystemSpecificationEditable> GetAllSystemsAndPlanets() => _universe.Systems;
    
    public SystemSpecificationEditable GetSystemAndPlanets(string systemName)
        => _universe.Systems.FirstOrDefault(x => x.Name == systemName) ?? throw new InvalidOperationException();
    
    public List<PlanetSpecificationEditable> GetPlanetsOfSystem(string systemName)
        => _universe.Systems.FirstOrDefault(x => x.Name == systemName)!.Planets;
    
    public PlanetSpecificationEditable GetPlanetOfSystem(string systemName, string planetName)
        => GetPlanetsOfSystem(systemName).FirstOrDefault(x => x.Name == planetName) ??
           throw new InvalidOperationException();
    
    public SystemSpecificationEditable GetRandomSystem()
    => _universe.Systems.MinBy(x => Guid.NewGuid())!;
}