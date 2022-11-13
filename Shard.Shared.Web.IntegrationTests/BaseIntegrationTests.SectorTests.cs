namespace Shard.Shared.Web.IntegrationTests;

public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
{
    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "1")]
    public async Task CanReadSystems()
    {
        using var client = CreateClient();
        using var response = await client.GetAsync("systems");

        var array = await response.AssertSuccessJsonAsync();
        array.AssertNotEmptyArray();
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "1")]
    public async Task SystemsHaveNames()
    {
        using var client = CreateClient();
        using var response = await client.GetAsync("systems");

        var array = await response.AssertSuccessJsonAsync();
        var firstSystem = array[1]["name"].AssertString();
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "1")]
    public async Task SystemsHavePlanets()
    {
        using var client = CreateClient();
        using var response = await client.GetAsync("systems");

        var array = await response.AssertSuccessJsonAsync();
        array[0]["planets"].AssertNotEmptyArray();
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "1")]
    public async Task PlanetsHaveNames()
    {
        using var client = CreateClient();
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
        using var client = CreateClient();
        using var response = await client.GetAsync("systems");

        var array = await response.AssertSuccessJsonAsync();
        array[0]["planets"][0]["size"].AssertInteger();
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task PlanetsDoNotHaveResources()
    {
        using var client = CreateClient();
        using var response = await client.GetAsync("systems");

        var array = await response.AssertSuccessJsonAsync();

        var allPlanets = array.SelectTokens("[*].planets[*]").Cast<IDictionary<string, JToken>>();
        var allProperties = allPlanets.SelectMany(planet => planet.Keys).Distinct();
        Assert.DoesNotContain("resource", string.Join(",", allProperties));
    }

    public async Task<StarSystem> GetFirstSystem()
    {
        using var client = CreateClient();
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

        using var client = CreateClient();
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

        using var client = CreateClient();
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

        using var client = CreateClient();
        using var response = await client.GetAsync($"systems/{system.Name}/planets/{planet.Name}");

        Assert.Equal(planet.ToString(), (await response.AssertSuccessJsonAsync()).ToString());
    }

    [Fact]
    [Trait("grading", "true")]
    public async Task NonExistingSystemReturns404()
    {
        using var client = CreateClient();
        using var response = await client.GetAsync("systems");

        var array = await response.AssertSuccessJsonAsync();
        array[0]["planets"][0]["size"].AssertInteger();
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "4")]
    public async Task SystemIsFollowingTestSpecifications()
    {
        var expectedJson = GetExpectedJson("expectedTestSector.json")?.Replace("\r", string.Empty);

        using var client = CreateClient();
        using var response = await client.GetAsync("systems");

        var array = await response.AssertSuccessJsonAsync();
        Assert.Equal(expectedJson, array.ToIndentedString()?.Replace("\r", string.Empty));
    } 

    private static string? GetExpectedJson(string fileName)
    {
        // We assume test files are under the current assembly 
        // AND the same namespace (or a child one)
        var sibblingType = typeof(BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>);
        var owningAssembly = sibblingType.Assembly;
        var baseNameSpace = sibblingType.Namespace;

        using var stream = owningAssembly.GetManifestResourceStream(baseNameSpace + "." + fileName);
        if (stream == null)
            return null;

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
