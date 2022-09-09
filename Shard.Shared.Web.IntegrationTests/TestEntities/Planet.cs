namespace Shard.Shared.Web.IntegrationTests.TestEntities;

public record Planet(JObjectAsserter Json)
{ 
    public Planet(JTokenAsserter json)
        : this(json.AssertObject())
    {
    }

    public string Name => Json["name"].AssertNonEmptyString();
    public override string ToString() => Json.ToString();
}
