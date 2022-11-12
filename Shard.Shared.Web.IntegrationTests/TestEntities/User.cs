namespace Shard.Shared.Web.IntegrationTests.TestEntities;

public record User(JObjectAsserter Json)
{ 
    public User(JTokenAsserter json)
        : this(json.AssertObject())
    {
    }

    public User(JToken json)
        : this(new JTokenAsserter(json))
    {
    }

    public string Id => Json["id"].AssertNonEmptyString();
    public string Pseudo => Json["pseudo"].AssertNonEmptyString();
    public DateTime DateOfCreation => Json["dateOfCreation"].AssertDateTime();
    public ResourcesQuantity ResourcesQuantity => new(Json["resourcesQuantity"]);

    public string Url => $"users/{Id}";

    public override string ToString() => Json.ToString();
}
