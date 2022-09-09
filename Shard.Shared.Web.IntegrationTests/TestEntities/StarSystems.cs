using System.Collections;

namespace Shard.Shared.Web.IntegrationTests.TestEntities;

public record StarSystems(JArrayAsserter Json) : IEnumerable<StarSystem>
{
    public StarSystems(JTokenAsserter json)
        : this(json.AssertArray())
    {
    }

    public StarSystem this[int index] => new(Json[index]);

    public IEnumerator<StarSystem> GetEnumerator()
        => Json.Children.Select(token => new StarSystem(token)).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => Json.ToString();
}