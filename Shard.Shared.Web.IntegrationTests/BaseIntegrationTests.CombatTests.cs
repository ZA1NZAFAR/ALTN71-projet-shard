using Shard.Shared.Web.IntegrationTests.TestEntities;
using System.Net;

namespace Shard.Shared.Web.IntegrationTests;

public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
{
    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task FighterVsCruiser_5sec_Nothing()
    {
        await fakeClock.SetNow(DateTime.Today);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var units = await CreateDuel(client, "fighter", "cruiser");
        await fakeClock.Advance(TimeSpan.FromSeconds(5));
        var (refreshedFighter, refreshedCruiser) = await GetDuelStatus(client, units);

        Assert.Equal(80, refreshedFighter.Health);
        Assert.Equal(400, refreshedCruiser.Health);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task FighterVsCruiser_6sec_FighterInflicts10dmg()
    {
        await fakeClock.SetNow(DateTime.Today);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var (_, originalCruiser) = await CreateDuel(client, "fighter", "cruiser");
        await fakeClock.Advance(TimeSpan.FromSeconds(6));
        var refreshedCruiser = await RefreshUnit(client, originalCruiser);

        Assert.Equal(390, refreshedCruiser.Health);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task FighterVsCruiser_6sec_CruiserInflicts40dmg()
    {
        await fakeClock.SetNow(DateTime.Today);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var (originalFighter, _) = await CreateDuel(client, "fighter", "cruiser");
        await fakeClock.Advance(TimeSpan.FromSeconds(6));
        var refreshedFighter = await RefreshUnit(client, originalFighter);

        Assert.Equal(40, refreshedFighter.Health);
    }



    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task FighterVsBomber_5sec_Nothing()
    {
        await fakeClock.SetNow(DateTime.Today);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var units = await CreateDuel(client, "fighter", "bomber");
        await fakeClock.Advance(TimeSpan.FromSeconds(5));
        var (refreshedFighter, refreshedBomber) = await GetDuelStatus(client, units);

        Assert.Equal(80, refreshedFighter.Health);
        Assert.Equal(50, refreshedBomber.Health);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task FighterVsBomber_6sec_FighterInflicts10dmg()
    {
        await fakeClock.SetNow(DateTime.Today);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var (_, originalBomber) = await CreateDuel(client, "fighter", "bomber");
        await fakeClock.Advance(TimeSpan.FromSeconds(6));
        var refreshedBomber = await RefreshUnit(client, originalBomber);

        Assert.Equal(40, refreshedBomber.Health);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task FighterVsBomber_6sec_BomberInflicts0dmg()
    {
        await fakeClock.SetNow(DateTime.Today);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var (originalFighter, _) = await CreateDuel(client, "fighter", "bomber");
        await fakeClock.Advance(TimeSpan.FromSeconds(6));
        var refreshedFighter = await RefreshUnit(client, originalFighter);

        Assert.Equal(80, refreshedFighter.Health);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task FighterVsBomber_6sec_atEndOFMinute_FighterInflicts10dmg()
    {
        await fakeClock.SetNow(DateTime.Today.AddSeconds(54));

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var (_, originalBomber) = await CreateDuel(client, "fighter", "bomber");
        await fakeClock.Advance(TimeSpan.FromSeconds(6));
        var refreshedBomber = await RefreshUnit(client, originalBomber);

        Assert.Equal(40, refreshedBomber.Health);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task FighterVsBomber_6sec_atEndOFMinute_BomberKillsFighter()
    {
        await fakeClock.SetNow(DateTime.Today.AddSeconds(54));

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var (originalFighter, _) = await CreateDuel(client, "fighter", "bomber");
        await fakeClock.Advance(TimeSpan.FromSeconds(6));

        await AssertUnitNotFound(client, originalFighter);
    }



    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task CruiserVsBomber_5sec_Nothing()
    {
        await fakeClock.SetNow(DateTime.Today);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var units = await CreateDuel(client, "cruiser", "bomber");
        await fakeClock.Advance(TimeSpan.FromSeconds(5));
        var (refreshedCruiser, refreshedBomber) = await GetDuelStatus(client, units);

        Assert.Equal(400, refreshedCruiser.Health);
        Assert.Equal(50, refreshedBomber.Health);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task CruiserVsBomber_6sec_CruiserInflicts6dmg()
    {
        await fakeClock.SetNow(DateTime.Today);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var (_, originalBomber) = await CreateDuel(client, "cruiser", "bomber");
        await fakeClock.Advance(TimeSpan.FromSeconds(6));
        var refreshedBomber = await RefreshUnit(client, originalBomber);

        Assert.Equal(46, refreshedBomber.Health);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task CruiserVsBomber_6sec_BomberInflicts0dmg()
    {
        await fakeClock.SetNow(DateTime.Today);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var (originalCruiser, _) = await CreateDuel(client, "cruiser", "bomber");
        await fakeClock.Advance(TimeSpan.FromSeconds(6));
        var refreshedCruiser = await RefreshUnit(client, originalCruiser);

        Assert.Equal(400, refreshedCruiser.Health);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task CruiserVsBomber_60sec_CruiserInflicts60dmg()
    {
        await fakeClock.SetNow(DateTime.Today);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var (_, originalBomber) = await CreateDuel(client, "cruiser", "bomber");
        await fakeClock.Advance(TimeSpan.FromSeconds(60));
        var refreshedBomber = await RefreshUnit(client, originalBomber);

        Assert.Equal(10, refreshedBomber.Health);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task CruiserVsBomber_60sec_BomberKillsFighter()
    {
        await fakeClock.SetNow(DateTime.Today);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var (originalCruiser, _) = await CreateDuel(client, "cruiser", "bomber");
        await fakeClock.Advance(TimeSpan.FromSeconds(60));

        await AssertUnitNotFound(client, originalCruiser);
    }



    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task BomberVsBomber_KillsEachOtherAfter60Sec()
    {
        await fakeClock.SetNow(DateTime.Today);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var units = await CreateDuel(client, "bomber", "bomber");
        await fakeClock.Advance(TimeSpan.FromSeconds(59));
        var (bomber1, bomber2) = await GetDuelStatus(client, units);

        Assert.Equal(50, bomber1.Health);
        Assert.Equal(50, bomber2.Health);

        await fakeClock.Advance(TimeSpan.FromSeconds(1));
        await AssertUnitNotFound(client, units.Item1);
        await AssertUnitNotFound(client, units.Item2);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task FighterVsFighter_KillsEachOtherAfter48Sec()
    {
        await fakeClock.SetNow(DateTime.Today);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var units = await CreateDuel(client, "fighter", "fighter");
        await fakeClock.Advance(TimeSpan.FromSeconds(47));
        var (fighter1, fighter2) = await GetDuelStatus(client, units);

        Assert.Equal(10, fighter1.Health);
        Assert.Equal(10, fighter2.Health);

        await fakeClock.Advance(TimeSpan.FromSeconds(1));
        await AssertUnitNotFound(client, units.Item1);
        await AssertUnitNotFound(client, units.Item2);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task CruiserVsCruiser_KillsEachOtherAfter60Sec()
    {
        await fakeClock.SetNow(DateTime.Today);

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var units = await CreateDuel(client, "cruiser", "cruiser");
        await fakeClock.Advance(TimeSpan.FromSeconds(59));
        var (cruiser1, cruiser2) = await GetDuelStatus(client, units);

        Assert.Equal(40, cruiser1.Health);
        Assert.Equal(40, cruiser2.Health);

        await fakeClock.Advance(TimeSpan.FromSeconds(1));
        await AssertUnitNotFound(client, units.Item1);
        await AssertUnitNotFound(client, units.Item2);
    }


    private static readonly IReadOnlyDictionary<string, int> expectedHealthPoints = new Dictionary<string, int>()
    {
        { "cruiser", 400 },
        { "bomber", 50 },
        { "fighter", 80 },
    };

    [Theory]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    [InlineData("cruiser", "cruiser", "fighter", "fighter")]
    [InlineData("cruiser", "cruiser", "bomber", "cruiser")]
    [InlineData("fighter", "cruiser", "fighter", "fighter")]
    [InlineData("fighter", "bomber", "fighter", "bomber")]
    [InlineData("bomber", "bomber", "fighter", "bomber")]
    [InlineData("bomber", "cruiser", "bomber", "cruiser")]
    public async Task CombatUnits_Priority(string soloType, string enemyType1, string enemyType2, string preferedType)
    {
        await fakeClock.SetNow(DateTime.Today.AddSeconds(54));

        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();
        var system = await GetRandomSystemOtherThan(null);

        await CreateUserWithUnit(client, soloType, system);
        var enemy1 = await CreateUserWithUnit(client, enemyType1, system);
        var enemy2 = await CreateUserWithUnit(client, enemyType2, system);

        await fakeClock.Advance(TimeSpan.FromSeconds(6));

        if (preferedType == enemyType1)
        {
            await AssertAttackedFirstAndNotSecond(client, enemy1, enemy2);
        }
        else
        {
            await AssertAttackedFirstAndNotSecond(client, enemy2, enemy1);
        }
    }

    private static async Task AssertAttackedFirstAndNotSecond(HttpClient client, Unit unit1, Unit unit2)
    {
        var enemy2 = await RefreshUnit(client, unit2);
        Assert.Equal(expectedHealthPoints[enemy2.Type], enemy2.Health);

        using var response = await client.GetAsync(unit1.Url);
        if (response.IsSuccessStatusCode)
        {
            var enemy1 = new Unit(unit1.UserPath, await response.AssertSuccessJsonAsync());
            Assert.NotEqual(expectedHealthPoints[enemy1.Type], enemy1.Health);
        }
    }

    private static async Task<(Unit, Unit)> GetDuelStatus(HttpClient client, (Unit, Unit) units)
    {
        return (await RefreshUnit(client, units.Item1), 
            await RefreshUnit(client, units.Item2));
    }

    private static async Task<Unit> RefreshUnit(HttpClient client, Unit unit)
    {
        using var response = await client.GetAsync(unit.Url);
        
        var refreshedUnit = new Unit(unit.UserPath, await response.AssertSuccessJsonAsync());
        Assert.Equal(unit.Id, refreshedUnit.Id);

        return refreshedUnit;
    }

    private static async Task AssertUnitNotFound(HttpClient client, Unit unit)
    {
        using var response = await client.GetAsync(unit.Url);
        await response.AssertStatusEquals(HttpStatusCode.NotFound);
    }

    private async Task<(Unit, Unit)> CreateDuel(HttpClient client, string unitType1, string unitType2)
    {
        var system = await GetRandomSystem();

        var unit1 = await CreateUserWithUnit(client, unitType1, system);
        var unit2 = await CreateUserWithUnit(client, unitType2, system);
        return (unit1, unit2);
    }

    private async Task<Unit> CreateUserWithUnit(HttpClient client, string unitType, string system)
    {
        var userPath = await CreateNewUserPath();
        return await CreateUnit(client, unitType, system, userPath);
    }

    private static async Task<Unit> CreateUnit(HttpClient client, string unitType, string system, string userPath)
    {
        var unitId = Guid.NewGuid().ToString();

        var response = await client.PutAsJsonAsync($"{userPath}/units/{unitId}", new
        {
            id = unitId,
            Type = unitType,
            system,
            resourcesQuantity = new { } // Some implementations might require this 
        });

        var unit = new Unit(userPath, await response.AssertSuccessJsonAsync());
        
        Assert.Equal(unitId, unit.Id);
        
        Assert.Equal(system, unit.System);
        Assert.Equal(system, unit.DestinationSystem);

        Assert.Null(unit.Planet);
        Assert.Null(unit.DestinationPlanet);

        return unit;
    }
}
