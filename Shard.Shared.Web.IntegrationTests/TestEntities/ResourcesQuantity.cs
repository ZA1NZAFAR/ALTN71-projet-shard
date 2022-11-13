namespace Shard.Shared.Web.IntegrationTests.TestEntities;

public record ResourcesQuantity(JObjectAsserter Json)
{ 
    public ResourcesQuantity(JTokenAsserter json)
        : this(json.AssertObject())
    {
    }

    public ResourcesQuantity(JToken json)
        : this(new JTokenAsserter(json))
    {
    }

    public int? Aluminium
    {
        get => Json["aluminium"].AssertNullableInteger();
        set => Json.SetPropertyValue("aluminium", value);
    }

    public int? Carbon
    {
        get => Json["carbon"].AssertNullableInteger();
        set => Json.SetPropertyValue("carbon", value);
    }

    public int? Gold
    {
        get => Json["gold"].AssertNullableInteger();
        set => Json.SetPropertyValue("gold", value);
    }

    public int? Iron
    {
        get => Json["iron"].AssertNullableInteger();
        set => Json.SetPropertyValue("iron", value);
    }

    public int? Oxygen
    {
        get => Json["oxygen"].AssertNullableInteger();
        set => Json.SetPropertyValue("oxygen", value);
    }

    public int? Titanium
    {
        get => Json["titanium"].AssertNullableInteger();
        set => Json.SetPropertyValue("titanium", value);
    }

    public int? Water
    {
        get => Json["water"].AssertNullableInteger();
        set => Json.SetPropertyValue("water", value);
    }

    public override string ToString() => Json.ToString();
}
