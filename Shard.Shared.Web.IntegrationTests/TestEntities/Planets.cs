using System.Collections;

namespace Shard.Shared.Web.IntegrationTests.TestEntities;

public record Planets(JArrayAsserter Json): IEnumerable<Planet>
{ 
    public Planets(JTokenAsserter json)
        : this(json.AssertArray())
    {
    }

    public Planet this[int index] => new(Json[index]);

    public IEnumerator<Planet> GetEnumerator()
        => Json.Children.Select(token => new Planet(token)).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => Json.ToString();

}
