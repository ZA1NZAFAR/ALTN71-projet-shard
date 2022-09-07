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

    private async Task<JObject> GetScout(string userPath)
    {
        using var client = factory.CreateClient();
        using var unitsResponse = await client.GetAsync($"{userPath}/units");
        await unitsResponse.AssertSuccessStatusCode();

        var units = await unitsResponse.Content.ReadAsAsync<JArray>();
        Assert.NotNull(units);
        Assert.Single(units);
        var unit = units[0].Value<JObject>();

        Assert.NotNull(unit);
        return unit;
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task CreatingUserCreatesScout()
    {
        var unit = await GetScout(await CreateNewUserPath());
        Assert.NotNull(unit["type"]);
        Assert.Equal(JTokenType.String, unit["type"]!.Type);
        Assert.Equal("scout", unit["type"]!.Value<string>());
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task CreatingUserCreatesScoutInSomeSystem()
    {
        var unit = await GetScout(await CreateNewUserPath());
        Assert.NotNull(unit["system"]);
        Assert.Equal(JTokenType.String, unit["system"]!.Type);

        var systemName = unit["system"]!.Value<string>();
        Assert.NotNull(systemName);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task CreatingUserCreatesScoutInSomeExistingSystem()
    {
        var unit = await GetScout(await CreateNewUserPath());
        Assert.NotNull(unit["system"]);
        var systemName = unit["system"]!.Value<string>();

        using var client = factory.CreateClient();
        using var response = await client.GetAsync("systems");
        await response.AssertSuccessStatusCode();

        var systems = await response.Content.ReadAsAsync<JArray>();
        Assert.NotNull(systems);
        var system = systems.SingleOrDefault(system => system["name"]?.Value<string>() == systemName);
        Assert.NotNull(system);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task GettingScoutStatusById()
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetScout(userPath);
        var unitId = unit["id"]?.Value<string>();
        Assert.NotNull(unitId);

        using var client = factory.CreateClient();
        using var response = await client.GetAsync($"{userPath}/units/{unitId}");
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
        var unitId = unit["id"]?.Value<string>();
        Assert.NotNull(unitId);

        using var client = factory.CreateClient();
        using var response = await client.GetAsync($"{userPath}/units/{unitId}z");
        await response.AssertStatusEquals(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task MoveScoutToOtherSystem()
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetScout(userPath);
        var unitId = unit["id"]?.Value<string>();
        Assert.NotNull(unitId);

        var currentSystem = unit["system"]?.Value<string>();
        Assert.NotNull(currentSystem);
        var destinationSystem = await GetRandomSystemOtherThan(currentSystem);

        using var client = factory.CreateClient();
        using var response = await client.PutAsJsonAsync($"{userPath}/units/{unitId}", new
        {
            id = unitId,
            type = "scout",
            system = destinationSystem
        });
        await response.AssertSuccessStatusCode();

        var unitAfterMove = await response.Content.ReadAsAsync<JObject>();
        Assert.NotNull(unitAfterMove);
        Assert.Equal(unitId, unitAfterMove["id"]?.Value<string>());
        Assert.Equal(destinationSystem, unitAfterMove["system"]?.Value<string>());
    }

    private async Task<string> GetRandomSystemOtherThan(string systemName)
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("systems");
        await response.AssertSuccessStatusCode();

        var systems = await response.Content.ReadAsAsync<JArray>();
        Assert.NotNull(systems);
        var system = systems.FirstOrDefault(system => system["name"]?.Value<string>() != systemName);
        Assert.NotNull(system);

        var name = system["name"]?.Value<string>();
        Assert.NotNull(name);
        return name;
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task MoveScoutToPlanet()
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetScout(userPath);
        var unitId = unit["id"]?.Value<string>();
        Assert.NotNull(unitId);

        var currentSystem = unit["system"]?.Value<string>();
        Assert.NotNull(currentSystem);
        var destinationPlanet = (await GetSomePlanetInSystem(currentSystem));

        using var client = factory.CreateClient();
        using var response = await client.PutAsJsonAsync($"{userPath}/units/{unitId}", new
        {
            id = unitId,
            type = "scout",
            system = currentSystem,
            planet = destinationPlanet
        });
        await response.AssertSuccessStatusCode();

        var unitAfterMove = await response.Content.ReadAsAsync<JObject>();
        Assert.NotNull(unitAfterMove);
        Assert.Equal(unitId, unitAfterMove["id"]?.Value<string>());
        Assert.Equal(currentSystem, unitAfterMove["system"]?.Value<string>());
        Assert.Equal(destinationPlanet, unitAfterMove["planet"]?.Value<string>());
    }

    private async Task<string> GetSomePlanetInSystem(string systemName)
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("systems");
        await response.AssertSuccessStatusCode();

        var systems = await response.Content.ReadAsAsync<JArray>();
        Assert.NotNull(systems);

        var system = systems.FirstOrDefault(system => system["name"]?.Value<string>() == systemName);
        Assert.NotNull(system);

        var planet = system["planets"]?.FirstOrDefault();
        Assert.NotNull(planet);

        var name = planet["name"]?.Value<string>();
        Assert.NotNull(name);
        return name;
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task AskingCurrentLocationsReturnsDetails()
    {
        var userPath = await CreateNewUserPath();
        var unit = await GetScout(userPath);
        var unitId = unit["id"]?.Value<string>();
        Assert.NotNull(unitId);
        
        var currentSystem = unit["system"]?.Value<string>();
        Assert.NotNull(currentSystem);
        var destinationPlanet = await GetSomePlanetInSystem(currentSystem);

        using var client = factory.CreateClient();
        using var moveResponse = await client.PutAsJsonAsync($"{userPath}/units/{unitId}", new
        {
            id = unitId,
            type = "scout",
            system = currentSystem,
            planet = destinationPlanet
        });
        await moveResponse.AssertSuccessStatusCode();

        using var scoutingResponse = await client.GetAsync($"{userPath}/units/{unitId}/location");
        await scoutingResponse.AssertSuccessStatusCode();

        var location = await scoutingResponse.Content.ReadAsAsync<JObject>();
        Assert.NotNull(location);
        Assert.Equal(currentSystem, location["system"]?.Value<string>());
        Assert.Equal(destinationPlanet, location["planet"]?.Value<string>());

        IDictionary<string, JToken?>? resources = location["resourcesQuantity"]?.Value<JObject>();
        Assert.NotNull(resources);

        foreach (var key in resources.Keys)
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
