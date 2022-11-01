namespace Shard.Shared.Web.IntegrationTests.Asserts; 

public class JTokenAsserter: BaseJTokenAsserter
{
    public JTokenAsserter(JToken token): base(token)
    {
    }

    public JTokenAsserter this[int index]
        => AssertArray()[index];

    public JTokenAsserter this[string name]
        => AssertObject()[name];

    public JArrayAsserter AssertArray()
    {
        Assert.Equal(JTokenType.Array, Token.Type);
        return new(Token);
    }

    public void AssertNotEmptyArray()
        => AssertArray().AssertNotEmpty();

    public JObjectAsserter AssertObject()
    {
        Assert.Equal(JTokenType.Object, Token.Type);
        return new(Token);
    }

    public string? AssertString()
    {
        Assert.Contains(Token.Type, new[] { JTokenType.String, JTokenType.Null });
        return Token.Value<string>();
    }

    public DateTime? AssertNullableDateTime()
    {
        Assert.Contains(Token.Type, new[] { JTokenType.Date, JTokenType.Null });
        return Token.Value<DateTime?>();
    }

    public bool AssertBoolean()
    {
        Assert.Equal(JTokenType.Boolean, Token.Type);
        return Token.Value<bool>();
    }

    public string AssertNonEmptyString()
    {
        Assert.Equal(JTokenType.String, Token.Type);
        var value = Token.Value<string>();
        Assert.NotNull(value);
        Assert.NotEmpty(value);
        return value;
    }

    public int AssertInteger()
    {
        Assert.Equal(JTokenType.Integer, Token.Type);
        return Token.Value<int>();
    }
}
