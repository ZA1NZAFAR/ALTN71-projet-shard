namespace Shard.Shared.Web.IntegrationTests.TestEntities;

public record Unit(JObjectAsserter Json)
{ 
    public Unit(JTokenAsserter json)
        : this(json.AssertObject())
    {
    }

    public string Id => Json["id"].AssertNonEmptyString();
    public string BaseUrl => $"users/{Id}";
    public string Type => Json["type"].AssertNonEmptyString();
    public string System => Json["system"].AssertNonEmptyString();
    public string? Planet => Json["planet"].AssertString();

    public override string ToString() => Json.ToString();
}
