using Microsoft.AspNetCore.Mvc;
using Shard.Api.Services;
using Shard.Shared.Core;

namespace Shard.Api.Controllers;

public class CelestialController
{
    private readonly ICelestialService _celestialService;

    public CelestialController(ICelestialService celestialService)
        => _celestialService = celestialService;


    [HttpGet("Systems")]
    public IReadOnlyList<SystemSpecification> GetAllSystemsAndPlanetsController() =>
        _celestialService.getAllSystemsAndPlanets();


    [HttpGet("Systems/{systemName}")]
    public SystemSpecification GetSystemAndPlanetsController(string systemName) =>
        _celestialService.getSystemAndPlanets(systemName);


    [HttpGet("Systems/{systemName}/planets")]
    public IReadOnlyList<PlanetSpecification> GetPlanetsOfSystemController(string systemName) =>
        _celestialService.getPlanetsOfSystem(systemName);

    [HttpGet("Systems/{systemName}/planets/{planetName}")]
    public PlanetSpecification GetPlanetOfSystemController(string systemName, string planetName) =>
        _celestialService.getPlanetOfSystem(systemName, planetName);
}