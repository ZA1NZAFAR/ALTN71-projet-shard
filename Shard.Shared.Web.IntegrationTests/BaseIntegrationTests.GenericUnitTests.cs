using System.Net;

namespace Shard.Shared.Web.IntegrationTests;

public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
{
    private async Task<Unit> GetSingleUnitOfType(string userPath, string unitType)
    {
        using var client = factory.CreateClient();
        using var unitsResponse = await client.GetAsync($"{userPath}/units");
        await unitsResponse.AssertSuccessStatusCode();

        var units = await unitsResponse.AssertSuccessJsonAsync();
        var token = units.SelectTokens($"[?(@.type=='{unitType}')]").FirstOrDefault();
        Assert.NotNull(token);
        return new(token);
    }

    private async Task CreatingUserCreatesOneUnitOfType(string unitType)
    {
        var unit = await GetSingleUnitOfType(await CreateNewUserPath(), unitType);
        Assert.Equal(unitType, unit.Type);
    }

    public async Task GettingUnitStatusById(string unitType)
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetSingleUnitOfType(userPath, unitType);

        using var client = factory.CreateClient();
        using var response = await client.GetAsync($"{userPath}/units/{unit.Id}");
        await response.AssertSuccessStatusCode();

        var unit2 = await response.Content.ReadAsAsync<JObject>();
        Assert.Equal(unit.ToString(), unit2?.ToString());
    }

    public async Task GettingUnitStatusWithWrongIdReturns404(string unitType)
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetSingleUnitOfType(userPath, unitType);

        using var client = factory.CreateClient();
        using var response = await client.GetAsync($"{userPath}/units/{unit.Id}z");
        await response.AssertStatusEquals(HttpStatusCode.NotFound);
    }

    public async Task MoveUnitToOtherSystem(string unitType)
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetSingleUnitOfType(userPath, unitType);

        var destinationSystem = await GetRandomSystemOtherThan(unit.System);
        unit.System = destinationSystem;

        using var client = factory.CreateClient();
        using var response = await client.PutTestEntityAsync($"{userPath}/units/{unit.Id}", unit);

        var unitAfterMove = new Unit(await response.AssertSuccessJsonAsync());
        Assert.NotNull(unitAfterMove);
        Assert.Equal(unit.Id, unitAfterMove.Id);
        Assert.Equal(destinationSystem, unitAfterMove.System);
    }

    private async Task<string> GetRandomSystemOtherThan(string systemName)
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("systems");

        var systems = new StarSystems(await response.AssertSuccessJsonAsync());
        var system = systems.First(system => system.Name != systemName);

        return system.Name;
    }

    public async Task MoveUnitToPlanet(string unitType)
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetSingleUnitOfType(userPath, unitType);

        var destinationPlanet = await GetSomePlanetInSystem(unit.System);
        unit.Planet = destinationPlanet;
        testOutputHelper.WriteLine(unit.Json.ToString());

        using var client = factory.CreateClient();
        using var response = await client.PutTestEntityAsync($"{userPath}/units/{unit.Id}", unit);

        var unitAfterMove = new Unit(await response.AssertSuccessJsonAsync());
        Assert.Equal(unit.Id, unitAfterMove.Id);
        Assert.Equal(unit.System, unitAfterMove.System);
        Assert.Equal(destinationPlanet, unitAfterMove.Planet);
    }

    private async Task<string> GetSomePlanetInSystem(string systemName)
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("systems");

        var systems = new StarSystems(await response.AssertSuccessJsonAsync());
        var system = systems.SingleOrDefault(system => system.Name == systemName);
        Assert.NotNull(system);

        var planet = system.Planets.FirstOrDefault();
        Assert.NotNull(planet);
        return planet.Name;
    }
}
