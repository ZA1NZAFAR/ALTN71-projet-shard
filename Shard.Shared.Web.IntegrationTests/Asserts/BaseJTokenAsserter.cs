using System.Collections;

namespace Shard.Shared.Web.IntegrationTests.Asserts;

public class BaseJTokenAsserter
{
    private readonly JToken token;
    protected JToken Token => token;

    public BaseJTokenAsserter(JToken token)
    {
        this.token = token;
    }

    public IEnumerable<JToken> SelectTokens(string jsonPath)
        => token.SelectTokens(jsonPath);

    public override string ToString()
        => token.ToString();

    public IEnumerable<JTokenAsserter> Children
        => Token.Select(childToken => new JTokenAsserter(childToken));
}
