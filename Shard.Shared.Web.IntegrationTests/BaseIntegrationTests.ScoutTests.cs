namespace Shard.Shared.Web.IntegrationTests;

public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
{
    private async Task<string> CreateNewUserPath()
    {
        var userId = Guid.NewGuid().ToString();
        using var client = CreateClient();
        using var userCreationResponse = await client.PutAsJsonAsync("users/" + userId, new
        {
            id = userId,
            pseudo = "johny"
        });
        await userCreationResponse.AssertSuccessStatusCode();

        return "users/" + userId;
    }

    private Task<Unit> GetScout(string userPath)
        => GetSingleUnitOfType(userPath, "scout");

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public Task CreatingUserCreatesScout()
        => CreatingUserCreatesOneUnitOfType("scout");

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

        using var client = CreateClient();
        using var response = await client.GetAsync("systems");

        var systems = new StarSystems(await response.AssertSuccessJsonAsync());
        var system = systems.SingleOrDefault(system => system.Name == unit.System);
        Assert.NotNull(system);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public Task GettingScoutStatusById()
        => GettingUnitStatusById("scout");

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public Task GettingScoutStatusWithWrongIdReturns404()
        => GettingUnitStatusWithWrongIdReturns404("scout");

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public Task PutNonExistingScoutAsUnauthenticated()
        => PutNonExistingUnitAsUnauthenticated("scout");

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public Task PutNonExistingScoutAsAdministrator()
        => PutNonExistingUnitAsAdministrator("scout");

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public Task MoveScoutToOtherSystem()
        => MoveUnitToOtherSystem("scout");

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public Task MoveScoutToPlanet()
        => MoveUnitToPlanet("scout");

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task AskingCurrentLocationsReturnsDetails()
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetScout(userPath);
        
        var destinationPlanet = await GetSomePlanetInSystem(unit.System);

        unit.DestinationPlanet = destinationPlanet;

        using var client = CreateClient();
        using var moveResponse = await client.PutTestEntityAsync(unit.Url, unit);
        await moveResponse.AssertSuccessStatusCode();

        await fakeClock.Advance(new TimeSpan(0, 0, 15));

        using var scoutingResponse = await client.GetAsync($"{unit.Url}/location");

        var location = (await scoutingResponse.AssertSuccessJsonAsync()).AssertObject();
        Assert.Equal(unit.System, location["system"].AssertString());
        Assert.Equal(destinationPlanet, location["planet"].AssertString());

        AssertResourcesQuantity(location);
    }

    private static void AssertResourcesQuantity(JObjectAsserter data)
    {
        foreach (var key in data["resourcesQuantity"].AssertObject().Keys)
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

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public Task GetScout_IfMoreThan2secAway_DoesNotWait()
        => GetUnit_IfMoreThan2secAway_DoesNotWait("scout");

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public Task GetScout_IfLessOrEqualThan2secAway_Waits()
        => GetUnit_IfLessOrEqualThan2secAway_Waits("scout");

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public Task GetScout_IfLessOrEqualThan2secAway_WaitsUntilArrived()
        => GetUnit_IfLessOrEqualThan2secAway_WaitsUntilArrived("scout");
}
