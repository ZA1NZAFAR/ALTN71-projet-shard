namespace Shard.Shared.Web.IntegrationTests.Asserts; 

public class JArrayAsserter: BaseJTokenAsserter
{
    public JArrayAsserter(JToken token): base(token)
    {
    }

    public void AssertNotEmpty()
        => Assert.NotEmpty(Token);

    public JTokenAsserter AssertSingle()
    { 
        Assert.Single(Token);
        return new(Token.Single());
    }

    public JTokenAsserter AssertHasItem(int index)
    {
        var childToken = Token[index];
        Assert.NotNull(childToken);

        return new(childToken);
    }

    public JTokenAsserter this[int index]
        => AssertHasItem(index);
}
