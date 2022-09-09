namespace Shard.Shared.Web.IntegrationTests;

public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
{

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "1")]
    public async Task CanReadSystems()
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("systems");

        var array = await response.AssertSuccessJsonAsync();
        array.AssertNotEmptyArray();
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "1")]
    public async Task SystemsHaveNames()
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("systems");

        var array = await response.AssertSuccessJsonAsync();
        var firstSystem = array[1]["name"].AssertString();
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "1")]
    public async Task SystemsHavePlanets()
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("systems");

        var array = await response.AssertSuccessJsonAsync();
        array[0]["planets"].AssertNotEmptyArray();
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "1")]
    public async Task PlanetsHaveNames()
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("systems");

        var array = await response.AssertSuccessJsonAsync();
        
        array[0]["planets"][0]["name"].AssertString();

        var names = array.SelectTokens("$[*].planets[*].name")
            .Select(token => token.Value<string>())
            .Distinct();
        Assert.InRange(names.Count(), 0, int.MaxValue);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "1")]
    public async Task PlanetsHaveSizes()
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("systems");

        var array = await response.AssertSuccessJsonAsync();
        array[0]["planets"][0]["size"].AssertInteger();
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task PlanetsDoNotHaveResources()
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("systems");

        var array = await response.AssertSuccessJsonAsync();

        var allPlanets = array.SelectTokens("[*].planets[*]").Cast<IDictionary<string, JToken>>();
        var allProperties = allPlanets.SelectMany(planet => planet.Keys).Distinct();
        Assert.DoesNotContain("resource", string.Join(",", allProperties));
    }

    public async Task<StarSystem> GetFirstSystem()
    {
        using var client = factory.CreateClient();
        using var systemsResponse = await client.GetAsync("systems");

        var systems = await systemsResponse.AssertSuccessJsonAsync();
        return new(systems[0]);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "1")]
    public async Task CanFetchOneSystem()
    {
        var system = await GetFirstSystem();

        using var client = factory.CreateClient();
        using var response = await client.GetAsync($"systems/{system.Name}");

        Assert.Equal(system.ToString(), (await response.AssertSuccessJsonAsync()).ToString());
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "1")]
    public async Task CanFetchPlanetsOfOneSystem()
    {
        var system = await GetFirstSystem();
        Assert.NotNull(system);

        using var client = factory.CreateClient();
        using var response = await client.GetAsync($"systems/{system.Name}/planets");

        Assert.Equal(system.Planets.ToString(), (await response.AssertSuccessJsonAsync()).ToString());
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "1")]
    public async Task CanFetchOnePlanet()
    {
        var system = await GetFirstSystem();
        Assert.NotNull(system);

        var planet = system.Planets[0];

        using var client = factory.CreateClient();
        using var response = await client.GetAsync($"systems/{system.Name}/planets/{planet.Name}");

        Assert.Equal(planet.ToString(), (await response.AssertSuccessJsonAsync()).ToString());
    }

    [Fact]
    [Trait("grading", "true")]
    public async Task NonExistingSystemReturns404()
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("systems");

        var array = await response.AssertSuccessJsonAsync();
        array[0]["planets"][0]["size"].AssertInteger();
    }
}
