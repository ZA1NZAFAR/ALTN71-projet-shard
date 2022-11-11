using System.Net;

namespace Shard.Shared.Web.IntegrationTests;

public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
{
    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task BuildingStarportThenFetchingAllBuildingsIncludesStarport()
    {
        using var client = CreateClient();
        var (userPath, _, originalBuilding) = await BuildStarport(client);

        var response = await client.GetAsync($"{userPath}/buildings");
        await response.AssertSuccessStatusCode();

        var buildings = (await response.AssertSuccessJsonAsync()).AssertArray();
        var building = buildings.AssertSingle().AssertObject();
        Assert.Equal(originalBuilding.ToString(), building.ToString());
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task BuildingStarportThenFetchingBuildingByIdReturnsStarport()
    {
        using var client = CreateClient();
        var (userPath, _, originalBuilding) = await BuildStarport(client);
        var building = await RefreshBuilding(client, userPath, originalBuilding);

        Assert.Equal(originalBuilding.ToString(), building.ToString());
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task BuildingStarportThenWaiting4MinReturnsUnbuiltStarport()
    {
        using var client = CreateClient();
        var (userPath, _, originalBuilding) = await BuildStarport(client);

        await fakeClock.Advance(TimeSpan.FromMinutes(4));
        var building = await RefreshBuilding(client, userPath, originalBuilding);

        Assert.False(building.IsBuilt);
        Assert.Equal(fakeClock.Now.AddMinutes(1), building.EstimatedBuildTime);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task BuildingStarportThenWaiting5MinReturnsBuiltStarport()
    {
        using var client = CreateClient();
        var (userPath, _, originalBuilding) = await BuildStarport(client);

        await fakeClock.Advance(TimeSpan.FromMinutes(5));
        var building = await RefreshBuilding(client, userPath, originalBuilding);

        Assert.True(building.IsBuilt);
        Assert.Null(building.EstimatedBuildTime);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutOnBuiltStarportImmediatlyReturnsOne()
    {
        using var client = CreateClient();
        var (userPath, _, originalBuilding) = await BuildAndWaitStarportAsync(client);

        var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding.Id}/queue", new
        {
            type = "scout"
        });
        await response.AssertSuccessStatusCode();

        var unit = new Unit(await response.AssertSuccessJsonAsync());
        Assert.NotNull(unit.Id);
        Assert.Equal("scout", unit.Type);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutOnBuiltStarportCost5Carbon5Iron()
    {
        using var client = CreateClient();
        var (userPath, _, originalBuilding) = await BuildAndWaitStarportAsync(client);

        await AssertResourceQuantity(client, userPath, "carbon", 20);
        await AssertResourceQuantity(client, userPath, "iron", 10);

        var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding.Id}/queue", new
        {
            type = "scout"
        });
        await response.AssertSuccessStatusCode();
        await AssertResourceQuantity(client, userPath, "carbon", 15);
        await AssertResourceQuantity(client, userPath, "iron", 5);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingBuilderOnBuiltStarportCost5Carbon10Iron()
    {
        using var client = CreateClient();
        var (userPath, _, originalBuilding) = await BuildAndWaitStarportAsync(client);

        await AssertResourceQuantity(client, userPath, "carbon", 20);
        await AssertResourceQuantity(client, userPath, "iron", 10);

        var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding.Id}/queue", new
        {
            type = "builder"
        });
        await response.AssertSuccessStatusCode();
        await AssertResourceQuantity(client, userPath, "carbon", 15);
        await AssertResourceQuantity(client, userPath, "iron", 0);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutForInvalidUserReturns404()
    {
        using var client = CreateClient();
        var (userPath, _, originalBuilding) = await BuildAndWaitStarportAsync(client);

        var response = await client.PostAsJsonAsync($"{userPath}z/buildings/{originalBuilding.Id}/queue", new
        {
            type = "scout"
        });
        await response.AssertStatusEquals(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutForInvalidBuildingReturns404()
    {
        using var client = CreateClient();
        var (userPath, _, originalBuilding) = await BuildAndWaitStarportAsync(client);

        var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding.Id}z/queue", new
        {
            type = "scout"
        });
        await response.AssertStatusEquals(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutOnMineReturns400()
    {
        using var client = CreateClient();
        var (userPath, _, originalBuilding) = await BuildMine(client);

        await fakeClock.Advance(TimeSpan.FromMinutes(5));
        var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding.Id}/queue", new
        {
            type = "scout"
        });
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutOnUnBuiltStarportReturns400()
    {
        using var client = CreateClient();
        var (userPath, _, originalBuilding) = await BuildStarport(client);

        var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding.Id}/queue", new
        {
            type = "scout"
        });
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
    }
}
