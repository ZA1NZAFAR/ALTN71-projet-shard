using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Shard.Shared.Web.IntegrationTests;

internal class ExtraAssert
{
    public static void NotEmpty([NotNull] IEnumerable? collection)
    {
        Assert.NotNull(collection);
        Assert.NotEmpty(collection);
    }
}
