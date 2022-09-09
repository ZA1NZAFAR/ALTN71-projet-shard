using Newtonsoft.Json;

namespace Shard.Shared.Web.IntegrationTests.Asserts; 

public static class Extensions
{
    public static async Task<JTokenAsserter> AssertJsonAsync(this HttpContent content)
    {
        Assert.InRange(content.Headers.ContentLength ?? -1, 1, long.MaxValue);
        
        using Stream contentStream = await content.ReadAsStreamAsync();
        using StreamReader streamReader = new(contentStream);
        using JsonTextReader jsonReader = new(streamReader);

        var token = await JToken.LoadAsync(jsonReader);
        Assert.NotNull(token);
        return new JTokenAsserter(token);
    }

    public static async Task<JTokenAsserter> AssertSuccessJsonAsync(this HttpResponseMessage response)
    {
        await response.AssertSuccessStatusCode();
        return await response.Content.AssertJsonAsync();
    }
}
