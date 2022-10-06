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
}
