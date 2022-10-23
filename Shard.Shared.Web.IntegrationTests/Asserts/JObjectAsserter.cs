namespace Shard.Shared.Web.IntegrationTests.Asserts; 

public class JObjectAsserter : BaseJTokenAsserter
{
    public JObjectAsserter(JToken token): base(token)
    {
    }

    public JTokenAsserter AssertObjectHasProperty(string name)
    {
        var childToken = Token[name];
        Assert.NotNull(childToken);

        return new(childToken);
    }

    public void AssertNullOrMissingProperty(string name)
    {
        var childToken = Token[name];

        Assert.True(childToken == null || childToken.Type == JTokenType.Null);
    }

    public JTokenAsserter this[string name]
        => AssertObjectHasProperty(name);

    public JTokenAsserter? GetPropertyOrNull(string name)
    {
        var childToken = Token[name];
        if (childToken == null)
            return null;

        return new(childToken);
    }

    public void SetPropertyValue(string property, string? value)
        => Token[property] = value;

    public ICollection<string> Keys => ((IDictionary<string, JToken?>)Token).Keys;
}
