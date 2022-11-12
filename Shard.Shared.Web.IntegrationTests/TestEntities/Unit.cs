namespace Shard.Shared.Web.IntegrationTests.TestEntities;

public record Unit(string UserPath, JObjectAsserter Json)
{ 
    public Unit(string UserPath, JTokenAsserter json)
        : this(UserPath, json.AssertObject())
    {
    }

    public Unit(string UserPath, JToken json)
        : this(UserPath, new JTokenAsserter(json))
    {
    }

    public string Id => Json["id"].AssertNonEmptyString();
    public string Url => $"{UserPath}/units/{Id}";
    public string BuildUrl => $"{UserPath}/buildings";
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
    public int Health => Json["health"].AssertInteger();

    public override string ToString() => Json.ToString();
}
