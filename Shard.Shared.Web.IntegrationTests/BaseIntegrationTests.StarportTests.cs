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
        var (originalBuilding, _) = await BuildStarport(client);

        var response = await client.GetAsync($"{originalBuilding.UserPath}/buildings");
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
        var (originalBuilding, _) = await BuildStarport(client);
        var building = await RefreshBuilding(client, originalBuilding);

        Assert.Equal(originalBuilding.ToString(), building.ToString());
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task BuildingStarportThenWaiting4MinReturnsUnbuiltStarport()
    {
        using var client = CreateClient();
        var (originalBuilding, _) = await BuildStarport(client);

        await fakeClock.Advance(TimeSpan.FromMinutes(4));
        var building = await RefreshBuilding(client, originalBuilding);

        Assert.False(building.IsBuilt);
        Assert.Equal(fakeClock.Now.AddMinutes(1), building.EstimatedBuildTime);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task BuildingStarportThenWaiting5MinReturnsBuiltStarport()
    {
        using var client = CreateClient();
        var (originalBuilding, _) = await BuildStarport(client);

        await fakeClock.Advance(TimeSpan.FromMinutes(5));
        var building = await RefreshBuilding(client, originalBuilding);

        Assert.True(building.IsBuilt);
        Assert.Null(building.EstimatedBuildTime);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutOnBuiltStarportImmediatlyReturnsOne()
    {
        using var client = CreateClient();
        var originalBuilding = await BuildAndWaitStarportAsync(client);

        var response = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
        {
            type = "scout"
        });
        await response.AssertSuccessStatusCode();

        var unit = new Unit(originalBuilding.UserPath, await response.AssertSuccessJsonAsync());
        Assert.NotNull(unit.Id);
        Assert.Equal("scout", unit.Type);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutOnBuiltStarportCost5Carbon5Iron()
    {
        using var client = CreateClient();
        var originalBuilding = await BuildAndWaitStarportAsync(client);

        await AssertResourceQuantity(client, originalBuilding.UserPath, "carbon", 20);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "iron", 10);

        var response = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
        {
            type = "scout"
        });
        await response.AssertSuccessStatusCode();
        await AssertResourceQuantity(client, originalBuilding.UserPath, "carbon", 15);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "iron", 5);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingBuilderOnBuiltStarportCost5Carbon10Iron()
    {
        using var client = CreateClient();
        var originalBuilding = await BuildAndWaitStarportAsync(client);

        await AssertResourceQuantity(client, originalBuilding.UserPath, "carbon", 20);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "iron", 10);

        var response = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
        {
            type = "builder"
        });
        await response.AssertSuccessStatusCode();
        await AssertResourceQuantity(client, originalBuilding.UserPath, "carbon", 15);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "iron", 0);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutForInvalidUserReturns404()
    {
        using var client = CreateClient();
        var originalBuilding = await BuildAndWaitStarportAsync(client);

        var response = await client.PostAsJsonAsync($"{originalBuilding.UserPath}z/buildings/{originalBuilding.Id}/queue", new
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
        var originalBuilding = await BuildAndWaitStarportAsync(client);

        var response = await client.PostAsJsonAsync($"{originalBuilding.UserPath}/buildings/{originalBuilding.Id}z/queue", new
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
        var (originalBuilding, _) = await BuildMine(client);

        await fakeClock.Advance(TimeSpan.FromMinutes(5));
        var response = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
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
        var (originalBuilding, _) = await BuildStarport(client);

        var response = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
        {
            type = "scout"
        });
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutIfNotEnoughResourcesReturns400()
    {
        using var client = CreateClient();

        var originalBuilding = await BuildAndWaitStarportAsync(client);
        await ChangeUserResources(originalBuilding.UserPath, resoucesQuantity =>
        {
            resoucesQuantity.Carbon = 20;
            resoucesQuantity.Iron = 0;
        });

        var response = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
        {
            type = "scout"
        });
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutIfNotEnoughIronDoesNotSpendCarbon()
    {
        using var client = CreateClient();

        var originalBuilding = await BuildAndWaitStarportAsync(client);
        await ChangeUserResources(originalBuilding.UserPath, resoucesQuantity =>
        {
            resoucesQuantity.Carbon = 20;
            resoucesQuantity.Iron = 0;
        });

        var response = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
        {
            type = "scout"
        });
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "carbon", 20);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "iron", 0);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task QueuingScoutIfNotEnoughCarbonDoesNotSpendIron()
    {
        using var client = CreateClient();

        var originalBuilding = await BuildAndWaitStarportAsync(client);
        await ChangeUserResources(originalBuilding.UserPath, resoucesQuantity =>
        {
            resoucesQuantity.Carbon = 0;
            resoucesQuantity.Iron = 10;
        });

        var response = await client.PostAsJsonAsync(originalBuilding.QueueUrl, new
        {
            type = "scout"
        });
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "carbon", 0);
        await AssertResourceQuantity(client, originalBuilding.UserPath, "iron", 10);
    }
}
