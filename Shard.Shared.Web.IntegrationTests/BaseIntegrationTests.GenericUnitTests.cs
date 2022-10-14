using System.Net;

namespace Shard.Shared.Web.IntegrationTests;

public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
{
    private async Task<Unit> GetSingleUnitOfType(string userPath, string unitType)
    {
        using var client = CreateClient();
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

        using var client = CreateClient();
        using var response = await client.GetAsync($"{userPath}/units/{unit.Id}");
        await response.AssertSuccessStatusCode();

        var unit2 = await response.Content.ReadAsAsync<JObject>();
        Assert.Equal(unit.ToString(), unit2?.ToString());
    }

    public async Task GettingUnitStatusWithWrongIdReturns404(string unitType)
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetSingleUnitOfType(userPath, unitType);

        using var client = CreateClient();
        using var response = await client.GetAsync($"{userPath}/units/{unit.Id}z");
        await response.AssertStatusEquals(HttpStatusCode.NotFound);
    }

    public async Task MoveUnitToOtherSystem(string unitType)
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetSingleUnitOfType(userPath, unitType);

        var destinationSystem = await GetRandomSystemOtherThan(unit.System);
        unit.DestinationSystem = destinationSystem;
        unit.DestinationPlanet = null;

        using var client = CreateClient();
        using var response = await client.PutTestEntityAsync($"{userPath}/units/{unit.Id}", unit);

        var unitAfterMove = new Unit(await response.AssertSuccessJsonAsync());
        Assert.NotNull(unitAfterMove);
        Assert.Equal(unit.Id, unitAfterMove.Id);
        Assert.Equal(destinationSystem, unitAfterMove.DestinationSystem);
    }

    private async Task<string> GetRandomSystemOtherThan(string systemName)
    {
        using var client = CreateClient();
        using var response = await client.GetAsync("systems");

        var systems = new StarSystems(await response.AssertSuccessJsonAsync());
        var system = systems.First(system => system.Name != systemName);

        return system.Name;
    }

    public Task MoveUnitToPlanet(string unitType)
        => MoveUnitToPlanet(CreateClient(), unitType);

    private async Task<(string, string)> MoveUnitToPlanet(HttpClient client, string unitType)
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetSingleUnitOfType(userPath, unitType);

        var destinationPlanet = await GetSomePlanetInSystem(unit.System);
        unit.DestinationSystem = unit.System;
        unit.DestinationPlanet = destinationPlanet;

        using var response = await client.PutTestEntityAsync($"{userPath}/units/{unit.Id}", unit);

        var unitAfterMove = new Unit(await response.AssertSuccessJsonAsync());
        Assert.Equal(unit.Id, unitAfterMove.Id);
        Assert.Equal(unit.System, unitAfterMove.DestinationSystem);
        Assert.Equal(destinationPlanet, unitAfterMove.DestinationPlanet);

        return (userPath, unit.Id);
    }

    private async Task<(string, Unit)> SendUnitToPlanet(HttpClient client, string unitType)
    {
        var (userPath, unitId) = await MoveUnitToPlanet(client, unitType);

        await fakeClock.Advance(new TimeSpan(0, 0, 15));

        using var afterMoveResponse = await client.GetAsync($"{userPath}/units/{unitId}");
        return (userPath, new Unit(await afterMoveResponse.AssertSuccessJsonAsync()));
    }

    private async Task<string> GetSomePlanetInSystem(string systemName)
    {
        using var client = CreateClient();
        using var response = await client.GetAsync("systems");

        var systems = new StarSystems(await response.AssertSuccessJsonAsync());
        var system = systems.SingleOrDefault(system => system.Name == systemName);
        Assert.NotNull(system);

        var planet = system.Planets.FirstOrDefault();
        Assert.NotNull(planet);
        return planet.Name;
    }

    public async Task GetUnit_IfMoreThan2secAway_DoesNotWait(string unitType)
    {
        using var client = CreateClient();
        var (userPath, unitId) = await MoveUnitToPlanet(client, unitType);

        await fakeClock.Advance(new TimeSpan(0, 0, 13) - TimeSpan.FromTicks(1));

        var requestTask = client.GetAsync($"{userPath}/units/{unitId}");
        var delayTask = Task.Delay(500);
        var firstToSucceed = await Task.WhenAny(requestTask, delayTask);

        Assert.Same(requestTask, firstToSucceed);

        using var response = await requestTask;
        var unitAfterMove = new Unit(await response.AssertSuccessJsonAsync());
        Assert.Null(unitAfterMove.Planet);
	}

    public async Task GetUnit_IfLessOrEqualThan2secAway_Waits(string unitType)
    {
        using var client = CreateClient();
        var (userPath, unitId) = await MoveUnitToPlanet(client, unitType);

        await fakeClock.Advance(new TimeSpan(0, 0, 13));

        var requestTask = client.GetAsync($"{userPath}/units/{unitId}");
        var delayTask = Task.Delay(500);
        var firstToSucceed = await Task.WhenAny(requestTask, delayTask);

        Assert.Same(delayTask, firstToSucceed);
    }

    public async Task GetUnit_IfLessOrEqualThan2secAway_WaitsUntilArrived(string unitType)
    {
        using var client = CreateClient();
        var (userPath, unitId) = await MoveUnitToPlanet(client, unitType);

        await fakeClock.Advance(new TimeSpan(0, 0, 13));

        var requestTask = client.GetAsync($"{userPath}/units/{unitId}");
        await Task.Delay(500);

        await fakeClock.Advance(new TimeSpan(0, 0, 2));

        var delayTask = Task.Delay(500);
        var firstToSucceed = await Task.WhenAny(requestTask, delayTask);

        Assert.Same(requestTask, firstToSucceed);

        using var response = await requestTask;
        var unitAfterMove = new Unit(await response.AssertSuccessJsonAsync());
        Assert.NotNull(unitAfterMove.Planet);
    }
}