using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

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

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task CanForceResourcesForUser()
    {
        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var userPath = await CreateNewUserPath();
        var user = await GetUser(userPath, client);
        var updatedUser = await PutResources(client, user, resourcesQuantity =>
        {
            resourcesQuantity.Aluminium = 421;
            resourcesQuantity.Carbon = 422;
            resourcesQuantity.Gold = 423;
            resourcesQuantity.Iron = 424;
            resourcesQuantity.Oxygen = 425;
            resourcesQuantity.Titanium = 426;
            resourcesQuantity.Water = 427;
        });
        Assert.Equal(user.Id, updatedUser.Id);
        Assert.Equal(user.Pseudo, updatedUser.Pseudo);
        Assert.Equal(user.DateOfCreation, updatedUser.DateOfCreation);

        Assert.Equal(421, updatedUser.ResourcesQuantity.Aluminium);
        Assert.Equal(422, updatedUser.ResourcesQuantity.Carbon);
        Assert.Equal(423, updatedUser.ResourcesQuantity.Gold);
        Assert.Equal(424, updatedUser.ResourcesQuantity.Iron);
        Assert.Equal(425, updatedUser.ResourcesQuantity.Oxygen);
        Assert.Equal(426, updatedUser.ResourcesQuantity.Titanium);
        Assert.Equal(427, updatedUser.ResourcesQuantity.Water);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task CanForceSomeResourcesForUser()
    {
        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();
        
        var userPath = await CreateNewUserPath();
        var user = await GetUser(userPath, client);
        var updatedUser = await PutResources(client, user, resourcesQuantity =>
        {
            resourcesQuantity.Carbon = 422;
            resourcesQuantity.Gold = 423;
        });

        Assert.Equal(user.Id, updatedUser.Id);
        Assert.Equal(user.Pseudo, updatedUser.Pseudo);
        Assert.Equal(user.DateOfCreation, updatedUser.DateOfCreation);

        Assert.Equal(0, updatedUser.ResourcesQuantity.Aluminium);
        Assert.Equal(422, updatedUser.ResourcesQuantity.Carbon);
        Assert.Equal(423, updatedUser.ResourcesQuantity.Gold);
        Assert.Equal(10, updatedUser.ResourcesQuantity.Iron);
        Assert.Equal(50, updatedUser.ResourcesQuantity.Oxygen);
        Assert.Equal(0, updatedUser.ResourcesQuantity.Titanium);
        Assert.Equal(50, updatedUser.ResourcesQuantity.Water);
    }

    [Fact]
    [Trait("grading", "true")]
    [Trait("version", "5")]
    public async Task IgnoreResourcesUpdateIfNotAdmin()
    {
        using var client = CreateClient();

        var userPath = await CreateNewUserPath();
        var user = await GetUser(userPath, client);
        var updatedUser = await PutResources(client, user, resourcesQuantity =>
        {
            resourcesQuantity.Carbon = 422;
            resourcesQuantity.Gold = 423;
        });

        Assert.Equal(user.Id, updatedUser.Id);
        Assert.Equal(user.Pseudo, updatedUser.Pseudo);
        Assert.Equal(user.DateOfCreation, updatedUser.DateOfCreation);

        Assert.Equal(0, updatedUser.ResourcesQuantity.Aluminium);
        Assert.Equal(20, updatedUser.ResourcesQuantity.Carbon);
        Assert.Equal(0, updatedUser.ResourcesQuantity.Gold);
        Assert.Equal(10, updatedUser.ResourcesQuantity.Iron);
        Assert.Equal(50, updatedUser.ResourcesQuantity.Oxygen);
        Assert.Equal(0, updatedUser.ResourcesQuantity.Titanium);
        Assert.Equal(50, updatedUser.ResourcesQuantity.Water);
    }

    private async Task<User> ChangeUserResources(string userPath, Action<ResourcesQuantity> resourceMutator)
    {
        using var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

        var user = await GetUser(userPath, client);
        return await PutResources(client, user, resourceMutator);
    }

    private static async Task<User> GetUser(string userPath, HttpClient client)
    {
        using var response = await client.GetAsync(userPath);
        return new (await response.AssertSuccessJsonAsync());
    }

    private static async Task<User> PutResources(HttpClient client, User user, Action<ResourcesQuantity> resourceMutator)
    {
        resourceMutator(user.ResourcesQuantity);
        
        using var response = await client.PutTestEntityAsync(user.Url, user);
        return new (await response.AssertSuccessJsonAsync());
    }

    private static AuthenticationHeaderValue CreateAdminAuthorizationHeader()
        => new ("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("admin:password")));
}
