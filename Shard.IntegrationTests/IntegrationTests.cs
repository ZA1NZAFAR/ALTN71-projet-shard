using Microsoft.AspNetCore.Mvc.Testing;
using Shard.Api;
using Shard.Shared.Web.IntegrationTests;
using Xunit.Abstractions;

namespace Shard.IntegrationTests;

public class IntegrationTests : BaseIntegrationTests<Program>
{
    public IntegrationTests(
        WebApplicationFactory<Program> factory,
        ITestOutputHelper testOutputHelper)
        : base(factory, testOutputHelper)
    {
    }
}
