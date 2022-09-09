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

    public JTokenAsserter this[string name]
        => AssertObjectHasProperty(name);

    public ICollection<string> Keys => ((IDictionary<string, JToken?>)Token).Keys;
}
