namespace Shard.Shared.Web.IntegrationTests.TestEntities;

public record StarSystem(JObjectAsserter Json)
{ 
    public StarSystem(JTokenAsserter json)
        : this(json.AssertObject())
    {
    }

    public string Name => Json["name"].AssertNonEmptyString();
    public Planets Planets => new(Json["planets"]);
    public override string ToString() => Json.ToString();
}
