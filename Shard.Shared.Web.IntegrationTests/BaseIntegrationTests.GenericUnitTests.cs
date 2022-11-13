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
        return new(userPath, token);
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
        using var response = await client.GetAsync(unit.Url);
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

    public async Task PutNonExistingUnitAsUnauthenticated(string unitType)
    {
        using var client = CreateClient();
        var userPath = await CreateNewUserPath();
        var unitId = Guid.NewGuid();

        var originSystem = await GetRandomSystem();
        var originPlanet = await GetSomePlanetInSystem(originSystem);

        using var response = await client.PutAsJsonAsync($"{userPath}/units/{unitId}", new
        {
            id = unitId,
            Type = unitType,
            System = originSystem,
            Planet = originPlanet,
            resourcesQuantity = new { } // Some implementations might require this 
        });

        await response.AssertStatusEquals(HttpStatusCode.Unauthorized);
    }

    public async Task PutNonExistingUnitAsAdministrator(string unitType)
    {
        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var userPath = await CreateNewUserPath();
        var unitId = Guid.NewGuid();

        var originSystem = await GetRandomSystem();
        var originPlanet = await GetSomePlanetInSystem(originSystem);

        using var response = await client.PutAsJsonAsync($"{userPath}/units/{unitId}", new
        {
            id = unitId,
            Type = unitType,
            System = originSystem,
            Planet = originPlanet,
            resourcesQuantity = new { } // Some implementations might require this 
        });
        var unit = new Unit(userPath, await response.AssertSuccessJsonAsync());

        Assert.Equal(unitId.ToString(), unit.Id);
        Assert.Equal(originSystem, unit.System);
        Assert.Equal(originPlanet, unit.Planet);
        Assert.Equal(originSystem, unit.DestinationSystem);
        Assert.Equal(originPlanet, unit.DestinationPlanet);
    }


    public async Task MoveUnitToOtherSystem(string unitType)
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetSingleUnitOfType(userPath, unitType);

        var destinationSystem = await GetRandomSystemOtherThan(unit.System);
        unit.DestinationSystem = destinationSystem;
        unit.DestinationPlanet = null;

        using var client = CreateClient();
        using var response = await client.PutTestEntityAsync(unit.Url, unit);

        var unitAfterMove = new Unit(unit.UserPath, await response.AssertSuccessJsonAsync());
        Assert.NotNull(unitAfterMove);
        Assert.Equal(unit.Id, unitAfterMove.Id);
        Assert.Equal(destinationSystem, unitAfterMove.DestinationSystem);
    }

    private Task<string> GetRandomSystem() => GetRandomSystemOtherThan(null);


    private async Task<string> GetRandomSystemOtherThan(string? systemName)
    {
        using var client = CreateClient();
        using var response = await client.GetAsync("systems");

        var systems = new StarSystems(await response.AssertSuccessJsonAsync());
        var system = systems.First(system => system.Name != systemName);

        return system.Name;
    }

    public Task MoveUnitToPlanet(string unitType)
        => MoveUnitToPlanet(CreateClient(), unitType);

    private async Task<Unit> MoveUnitToPlanet(HttpClient client, string unitType)
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetSingleUnitOfType(userPath, unitType);

        var destinationPlanet = await GetSomePlanetInSystem(unit.System);
        unit.DestinationSystem = unit.System;
        unit.DestinationPlanet = destinationPlanet;

        using var response = await client.PutTestEntityAsync(unit.Url, unit);

        var unitAfterMove = new Unit(unit.UserPath, await response.AssertSuccessJsonAsync());
        Assert.Equal(unit.Id, unitAfterMove.Id);
        Assert.Equal(unit.System, unitAfterMove.DestinationSystem);
        Assert.Equal(destinationPlanet, unitAfterMove.DestinationPlanet);

        return unit;
    }

    private async Task<Unit> SendUnitToPlanet(HttpClient client, string unitType)
    {
        var unit = await MoveUnitToPlanet(client, unitType);

        await fakeClock.Advance(new TimeSpan(0, 0, 15));

        using var afterMoveResponse = await client.GetAsync(unit.Url);
        return new Unit(unit.UserPath, await afterMoveResponse.AssertSuccessJsonAsync());
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

    private async Task<Unit> SendUnitToSpecificPlanet(
        HttpClient client, string unitType, string destinationSystem, string destinationPlanet)
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetSingleUnitOfType(userPath, unitType);

        unit.DestinationSystem = destinationSystem;
        unit.DestinationPlanet = destinationPlanet;

        using var moveResponse = await client.PutTestEntityAsync(unit.Url, unit);
        await moveResponse.AssertSuccessStatusCode();

        await fakeClock.Advance(new TimeSpan(0, 1, 15));

        using var afterMoveResponse = await client.GetAsync(unit.Url);
        return new Unit(unit.UserPath, await afterMoveResponse.AssertSuccessJsonAsync());
    }

    public async Task GetUnit_IfMoreThan2secAway_DoesNotWait(string unitType)
    {
        using var client = CreateClient();
        var unit = await MoveUnitToPlanet(client, unitType);

        await fakeClock.Advance(new TimeSpan(0, 0, 13) - TimeSpan.FromTicks(1));

        var requestTask = client.GetAsync(unit.Url);
        var delayTask = Task.Delay(500);
        var firstToSucceed = await Task.WhenAny(requestTask, delayTask);

        Assert.Same(requestTask, firstToSucceed);

        using var response = await requestTask;
        var unitAfterMove = new Unit(unit.UserPath, await response.AssertSuccessJsonAsync());
        Assert.Null(unitAfterMove.Planet);
	}

    public async Task GetUnit_IfLessOrEqualThan2secAway_Waits(string unitType)
    {
        using var client = CreateClient();
        var unit = await MoveUnitToPlanet(client, unitType);

        await fakeClock.Advance(new TimeSpan(0, 0, 13));

        var requestTask = client.GetAsync(unit.Url);
        var delayTask = Task.Delay(500);
        var firstToSucceed = await Task.WhenAny(requestTask, delayTask);

        Assert.Same(delayTask, firstToSucceed);
    }

    public async Task GetUnit_IfLessOrEqualThan2secAway_WaitsUntilArrived(string unitType)
    {
        using var client = CreateClient();
        var unit = await MoveUnitToPlanet(client, unitType);

        await fakeClock.Advance(new TimeSpan(0, 0, 13));

        var requestTask = client.GetAsync(unit.Url);
        await Task.Delay(500);

        await fakeClock.Advance(new TimeSpan(0, 0, 2));

        var delayTask = Task.Delay(500);
        var firstToSucceed = await Task.WhenAny(requestTask, delayTask);

        Assert.Same(requestTask, firstToSucceed);

        using var response = await requestTask;
        var unitAfterMove = new Unit(unit.UserPath, await response.AssertSuccessJsonAsync());
        Assert.NotNull(unitAfterMove.Planet);
    }
}