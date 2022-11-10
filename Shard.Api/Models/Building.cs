namespace Shard.Api.Models;

public class Building
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string BuilderId { get; set; }
    public string System { get; set; }
    public string Planet { get; set; }
    public string ResourceCategory { get; set; }
    public bool IsBuilt { get; set; }
    public DateTime EstimatedBuildTime { get; set; }
    public Task BuildTask { get; set; }
}