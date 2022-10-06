namespace Shard.Shared.Web.IntegrationTests.TestEntities;

public record Unit(JObjectAsserter Json)
{ 
    public Unit(JTokenAsserter json)
        : this(json.AssertObject())
    {
    }

    public Unit(JToken json)
        : this(new JTokenAsserter(json))
    {
    }

    public string Id => Json["id"].AssertNonEmptyString();
    public string BaseUrl => $"users/{Id}";
    public string Type => Json["type"].AssertNonEmptyString();
    public string System
    {
        get => Json["system"].AssertNonEmptyString();
        set => Json.SetPropertyValue("system", value);
    }
    public string? Planet
    {
        get => Json["planet"].AssertString();
        set => Json.SetPropertyValue("planet", value);
    }
    public string DestinationSystem
    {
        get => Json["destinationSystem"].AssertNonEmptyString();
        set => Json.SetPropertyValue("destinationSystem", value);
    }
    public string? DestinationPlanet
    {
        get => Json["destinationPlanet"].AssertString();
        set => Json.SetPropertyValue("destinationPlanet", value);
    }

    public override string ToString() => Json.ToString();
}
