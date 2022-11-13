namespace Shard.Shared.Web.IntegrationTests.TestEntities;

public record Building(JObjectAsserter Json)
{ 
    public Building(JTokenAsserter json)
        : this(json.AssertObject())
    {
    }

    public Building(JToken json)
        : this(new JTokenAsserter(json))
    {
    }

    public string Id => Json["id"].AssertNonEmptyString();
    public string BaseUrl => $"users/{Id}";
    public string Type => Json["type"].AssertNonEmptyString();
    public bool IsBuilt => Json["isBuilt"].AssertBoolean();
    public DateTime? EstimatedBuildTime => Json.GetPropertyOrNull("estimatedBuildTime")?.AssertNullableDateTime();

    public override string ToString() => Json.ToString();
}
