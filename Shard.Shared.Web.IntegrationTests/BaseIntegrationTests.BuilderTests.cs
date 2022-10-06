using System.Net;

namespace Shard.Shared.Web.IntegrationTests;

public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
{
    private Task<Unit> GetBuilder(string userPath)
        => GetSingleUnitOfType(userPath, "builder");

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public Task CreatingUserCreatesBuilder()
        => CreatingUserCreatesOneUnitOfType("builder");

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public async Task CreatingUserCreatesBuilderInSameSystemThanScout()
    {
        var userPath = await CreateNewUserPath();
        var scout = await GetScout(userPath);
        var builder = await GetBuilder(userPath);
        Assert.NotNull(builder.System);
        Assert.Equal(scout.System, builder.System);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public Task GettingBuilderStatusById()
        => GettingUnitStatusById("builder");

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public Task GettingBuilderStatusWithWrongIdReturns404()
        => GettingUnitStatusWithWrongIdReturns404("builder");

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public Task MoveBuilderToOtherSystem()
        => MoveUnitToOtherSystem("builder");

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public Task MoveBuilderToPlanet()
        => MoveUnitToPlanet("builder");

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public async Task AskingCurrentLocationsOfBuilderDoesNotReturnDetails()
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetBuilder(userPath);

        var destinationPlanet = await GetSomePlanetInSystem(unit.System);

        unit.DestinationPlanet = destinationPlanet;
        using var client = CreateClient();
        using var moveResponse = await client.PutTestEntityAsync($"{userPath}/units/{unit.Id}", unit);
        await moveResponse.AssertSuccessStatusCode();

        await fakeClock.Advance(new TimeSpan(0, 0, 15));

        using var scoutingResponse = await client.GetAsync($"{userPath}/units/{unit.Id}/location");
        await scoutingResponse.AssertSuccessStatusCode();

        var location = (await scoutingResponse.AssertSuccessJsonAsync()).AssertObject();
        Assert.Equal(unit.System, location["system"].AssertString());
        Assert.Equal(destinationPlanet, location["planet"].AssertString());
        location.AssertNullOrMissingProperty("resourcesQuantity");
    }
		
    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public async Task CanBuildMineOnPlanet()
    {
        using var client = CreateClient();
        client.Timeout = TimeSpan.FromSeconds(1);
        var (userPath, builder) = await SendUnitToPlanet(client, "builder");

        var response = await client.PostAsJsonAsync($"{userPath}/buildings", new
        {
            builderId = builder.Id,
            type = "mine"
        });

        var building = (await response.AssertSuccessJsonAsync()).AssertObject();
        Assert.Equal("mine", building["type"].AssertString());
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public async Task BuildingMineReturnsMineWithLocation()
    {
        using var client = CreateClient();
        var (userPath, builder) = await SendUnitToPlanet(client, "builder");

        var response = await client.PostAsJsonAsync($"{userPath}/buildings", new
        {
            builderId = builder.Id,
            type = "mine"
        });
        await response.AssertSuccessStatusCode();

        var building = (await response.AssertSuccessJsonAsync()).AssertObject();
        Assert.Equal(builder.System, building["system"].AssertString());
        Assert.Equal(builder.Planet, building["planet"].AssertString());
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public async Task BuildingWithNoBodySends400()
    {
        using var client = CreateClient();
        var (userPath, builder) = await SendUnitToPlanet(client, "builder");

        var response = await client.PostAsJsonAsync<object?>($"{userPath}/buildings", null);
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public async Task BuildingWithIncorrectUserIdSends404()
    {
        using var client = CreateClient();
        var (userPath, builder) = await SendUnitToPlanet(client, "builder");

        var response = await client.PostAsJsonAsync($"{userPath}x/buildings", new
        {
            builderId = builder.Id,
            type = "mine"
        });
        await response.AssertStatusEquals(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public async Task BuildingWithNoBuilderIdSends400()
    {
        using var client = CreateClient();
        var (userPath, builder) = await SendUnitToPlanet(client, "builder");

        var response = await client.PostAsJsonAsync($"{userPath}/buildings", new
        {
            type = "mine"
        });
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public async Task BuildingWithIncorrectBuilderIdSends400()
    {
        using var client = CreateClient();
        var (userPath, builder) = await SendUnitToPlanet(client, "builder");

        var response = await client.PostAsJsonAsync($"{userPath}/buildings", new
        {
            builderId = builder.Id + "x",
            type = "mine"
        });
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public async Task BuildingWithIncorrectBuildingTypeSends400()
    {
        using var client = CreateClient();
        var (userPath, builder) = await SendUnitToPlanet(client, "builder");

        var response = await client.PostAsJsonAsync($"{userPath}/buildings", new
        {
            builderId = builder.Id,
            type = "enim"
        });
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public async Task BuildingWithUnitNotOverPlanetSends404()
    {
        using var client = CreateClient();

        var userPath = await CreateNewUserPath();
        var builder = await GetBuilder(userPath);

        var response = await client.PostAsJsonAsync($"{userPath}/buildings", new
        {
            builderId = builder.Id,
            type = "mine"
        });
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public Task GetBuilder_IfMoreThan2secAway_DoesNotWait()
        => GetUnit_IfMoreThan2secAway_DoesNotWait("builder");

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public Task GetBuilder_IfLessOrEqualThan2secAway_Waits()
        => GetUnit_IfLessOrEqualThan2secAway_Waits("builder");

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public Task GetBuilder_IfLessOrEqualThan2secAway_WaitsUntilArrived()
        => GetUnit_IfLessOrEqualThan2secAway_WaitsUntilArrived("builder");
}
