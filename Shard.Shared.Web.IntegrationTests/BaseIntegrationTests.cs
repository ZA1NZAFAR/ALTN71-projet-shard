using Microsoft.AspNetCore.Hosting; 
using Microsoft.AspNetCore.Mvc.Testing; 
using Microsoft.Extensions.Configuration; 
using Microsoft.Extensions.Logging; 
using Xunit.Abstractions;

namespace Shard.Shared.Web.IntegrationTests; 

public abstract partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory> 
    : IClassFixture<TWebApplicationFactory> 
    where TEntryPoint : class 
    where TWebApplicationFactory: WebApplicationFactory<TEntryPoint> 
{ 
    private readonly WebApplicationFactory<TEntryPoint> factory; 

    public BaseIntegrationTests(TWebApplicationFactory factory, ITestOutputHelper testOutputHelper) 
    { 
        this.factory = factory 
            .WithWebHostBuilder(builder => 
            { 
                builder.ConfigureAppConfiguration(RemoveAllReloadOnChange); 
                builder.ConfigureLogging( 
                    logging => logging.AddProvider(new XunitLoggerProvider(testOutputHelper))); 
            }); 
    } 

    private void RemoveAllReloadOnChange(WebHostBuilderContext context, IConfigurationBuilder configuration) 
    { 
        foreach (var source in configuration.Sources.OfType<FileConfigurationSource>()) 
            source.ReloadOnChange = false; 
    }
}

public abstract class BaseIntegrationTests<TEntryPoint>: BaseIntegrationTests<TEntryPoint, WebApplicationFactory<TEntryPoint>>
    where TEntryPoint : class
{
    public BaseIntegrationTests(WebApplicationFactory<TEntryPoint> factory, ITestOutputHelper testOutputHelper)
        : base(factory, testOutputHelper)
    { }
}
