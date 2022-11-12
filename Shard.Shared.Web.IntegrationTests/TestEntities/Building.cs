namespace Shard.Shared.Web.IntegrationTests.TestEntities;

public record Building(string UserPath, JObjectAsserter Json)
{ 
    public Building(string userPath, JTokenAsserter json)
        : this(userPath, json.AssertObject())
    {
    }

    public Building(string userPath, JToken json)
        : this(userPath, new JTokenAsserter(json))
    {
    }

    public string Id => Json["id"].AssertNonEmptyString();
    public string Url => $"{UserPath}/buildings/{Id}";
    public string QueueUrl => $"{Url}/queue";
    public string Type => Json["type"].AssertNonEmptyString();
    public string System => Json["system"].AssertNonEmptyString();
    public string Planet => Json["planet"].AssertNonEmptyString();
    public bool IsBuilt => Json["isBuilt"].AssertBoolean();
    public DateTime? EstimatedBuildTime => Json.GetPropertyOrNull("estimatedBuildTime")?.AssertNullableDateTime();

    public override string ToString() => Json.ToString();
}
