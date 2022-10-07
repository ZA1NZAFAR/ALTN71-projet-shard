using Microsoft.AspNetCore.Mvc;
using Shard.Api.Models;
using Shard.Api.Services;

namespace Shard.Api.Controllers;

public class CelestialController : Controller
{
    private readonly ICelestialService _celestialService;

    public CelestialController(ICelestialService celestialService)
        => _celestialService = celestialService;


    [HttpGet("Systems")]
    public List<SystemContainingResourcelessPlanets> GetAllSystemsAndPlanetsController()
        => _celestialService.getAllSystemsAndPlanets().Select(systemAndPlanet => new SystemContainingResourcelessPlanets(systemAndPlanet))
            .ToList();


    [HttpGet("Systems/{systemName}")]
    public SystemContainingResourcelessPlanets GetSystemAndPlanetsController(string systemName)
        => new(_celestialService.getSystemAndPlanets(systemName));


    [HttpGet("Systems/{systemName}/planets")]
    public List<ResourcelessPlanet> GetPlanetsOfSystemController(string systemName)
        => _celestialService.getPlanetsOfSystem(systemName)
            .Select(planetSpecification => new ResourcelessPlanet(planetSpecification)).ToList();


    [HttpGet("Systems/{systemName}/planets/{planetName}")]
    public ResourcelessPlanet GetPlanetOfSystemController(string systemName, string planetName) =>
        new(_celestialService.getPlanetOfSystem(systemName, planetName));
}