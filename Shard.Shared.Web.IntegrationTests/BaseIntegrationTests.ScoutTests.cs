using System.Net;

namespace Shard.Shared.Web.IntegrationTests;

public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
{
    private async Task<string> CreateNewUserPath()
    {
        var userId = Guid.NewGuid().ToString();
        using var client = factory.CreateClient();
        using var userCreationResponse = await client.PutAsJsonAsync("users/" + userId, new
        {
            id = userId,
            pseudo = "johny"
        });
        await userCreationResponse.AssertSuccessStatusCode();

        return "users/" + userId;
    }

    private async Task<Unit> GetScout(string userPath)
    {
        using var client = factory.CreateClient();
        using var unitsResponse = await client.GetAsync($"{userPath}/units");
        await unitsResponse.AssertSuccessStatusCode();

        var units = await unitsResponse.AssertSuccessJsonAsync();
        return new (units[0]);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task CreatingUserCreatesScout()
    {
        var unit = await GetScout(await CreateNewUserPath());
        Assert.Equal("scout", unit.Type);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task CreatingUserCreatesScoutInSomeSystem()
    {
        var unit = await GetScout(await CreateNewUserPath());
        Assert.NotNull(unit.System);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task CreatingUserCreatesScoutInSomeExistingSystem()
    {
        var unit = await GetScout(await CreateNewUserPath());

        using var client = factory.CreateClient();
        using var response = await client.GetAsync("systems");

        var systems = new StarSystems(await response.AssertSuccessJsonAsync());
        var system = systems.SingleOrDefault(system => system.Name == unit.System);
        Assert.NotNull(system);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task GettingScoutStatusById()
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetScout(userPath);

        using var client = factory.CreateClient();
        using var response = await client.GetAsync($"{userPath}/units/{unit.Id}");
        await response.AssertSuccessStatusCode();

        var unit2 = await response.Content.ReadAsAsync<JObject>();
        Assert.Equal(unit.ToString(), unit2?.ToString());
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task GettingScoutStatusWithWrongIdReturns404()
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetScout(userPath);

        using var client = factory.CreateClient();
        using var response = await client.GetAsync($"{userPath}/units/{unit.Id}z");
        await response.AssertStatusEquals(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task MoveScoutToOtherSystem()
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetScout(userPath);

        var destinationSystem = await GetRandomSystemOtherThan(unit.System);

        using var client = factory.CreateClient();
        using var response = await client.PutAsJsonAsync($"{userPath}/units/{unit.Id}", new
        {
            id = unit.Id,
            type = "scout",
            system = destinationSystem
        });

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

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task MoveScoutToPlanet()
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetScout(userPath);

        var destinationPlanet = await GetSomePlanetInSystem(unit.System);

        using var client = factory.CreateClient();
        using var response = await client.PutAsJsonAsync($"{userPath}/units/{unit.Id}", new
        {
            id = unit.Id,
            type = "scout",
            system = unit.System,
            planet = destinationPlanet
        });

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

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task AskingCurrentLocationsReturnsDetails()
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetScout(userPath);
        
        var destinationPlanet = await GetSomePlanetInSystem(unit.System);

        using var client = factory.CreateClient();
        using var moveResponse = await client.PutAsJsonAsync($"{userPath}/units/{unit.Id}", new
        {
            id = unit.Id,
            type = "scout",
            system = unit.System,
            planet = destinationPlanet
        });
        await moveResponse.AssertSuccessStatusCode();

        using var scoutingResponse = await client.GetAsync($"{userPath}/units/{unit.Id}/location");

        var location = (await scoutingResponse.AssertSuccessJsonAsync()).AssertObject();
        Assert.Equal(unit.System, location["system"].AssertString());
        Assert.Equal(destinationPlanet, location["planet"].AssertString());

        foreach (var key in location["resourcesQuantity"].AssertObject().Keys)
        {
            Assert.Contains(key, new[]
            {
                "carbon",
                "iron",
                "gold",
                "aluminium",
                "titanium",
                "water",
                "oxygen",
            });
        }
    }
}
