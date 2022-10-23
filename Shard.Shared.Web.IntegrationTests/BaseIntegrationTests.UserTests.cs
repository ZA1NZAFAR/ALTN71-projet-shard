using System.Net;

namespace Shard.Shared.Web.IntegrationTests;

public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
{
    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task CanGet404WhenQueryingUser()
    {
        using var client = CreateClient();
        using var response = await client.GetAsync("users/42");
        await response.AssertStatusEquals(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task CanCreateUser()
    {
        using var client = CreateClient();
        using var response = await client.PutAsJsonAsync("users/43", new
        {
            id = "43",
            pseudo = "johny"
        });

        var user = (await response.AssertSuccessJsonAsync()).AssertObject();
        Assert.Equal("43", user["id"].AssertString());
        Assert.Equal("johny", user["pseudo"].AssertString());
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task CreatingUserWithInconsistentIdFails()
    {
        using var client = CreateClient();
        using var response = await client.PutAsJsonAsync("users/44", new
        {
            id = "45",
            pseudo = "johny"
        });
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task CreatingUserWithLackOfBodyFails()
    {
        using var client = CreateClient();
        using var response = await client.PutAsJsonAsync<object?>("users/46", null);
        await response.AssertStatusEquals(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task CreatingUserWithInvalidIdFails()
    {
        using var client = CreateClient();
        using var response = await client.PutAsJsonAsync("users/'", new
        {
            id = "'",
            pseudo = "johny"
        });
        await response.AssertStatusCodeAmong(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "2")]
    public async Task CanFetchCreatedUser()
    {
        using var client = CreateClient();
        using var userCreationResponse = await client.PutAsJsonAsync("users/47", new
        {
            id = "47",
            pseudo = "johny"
        });
        await userCreationResponse.AssertSuccessStatusCode();

        using var getUserResponse = await client.GetAsync("users/47");

        var user = await getUserResponse.AssertSuccessJsonAsync();
        Assert.Equal("47", user["id"].AssertString());
        Assert.Equal("johny", user["pseudo"].AssertString());
	}

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public async Task CanFetchResourcesFromNewlyCreatedUser()
    {
        using var client = CreateClient();
        using var getUserResponse = await client.GetAsync(await CreateNewUserPath());

        var user = await getUserResponse.AssertSuccessJsonAsync();
        AssertResourcesQuantity(user.AssertObject());
    }

    [Theory]
    [InlineData("aluminium", 0)]
    [InlineData("carbon", 20)]
    [InlineData("gold", 0)]
    [InlineData("iron", 10)]
    [InlineData("oxygen", 50)]
    [InlineData("titanium", 0)]
    [InlineData("water", 50)]
    [Trait("grading", "true")]
    [Trait("version", "3")]
    public async Task GivesBasicResourcesToNewUser(string resourceName, int resourceQuantity)
    {
        using var client = CreateClient();
        var userPath = await CreateNewUserPath();
        await AssertResourceQuantity(client, userPath, resourceName, resourceQuantity);
    }

    private static async Task AssertResourceQuantity(HttpClient client, string userPath, string resourceName, int resourceQuantity)
    {
        var getUserResponse = await client.GetAsync(userPath);

        var user = await getUserResponse.AssertSuccessJsonAsync();
        Assert.Equal(resourceQuantity, user["resourcesQuantity"][resourceName].AssertInteger());
    }
}
