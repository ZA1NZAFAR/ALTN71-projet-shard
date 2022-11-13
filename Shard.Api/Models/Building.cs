using System.Text.Json.Serialization;

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
    public DateTime? EstimatedBuildTime { get; set; }
    public Task BuildTask { get; set; } = null!;


    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public DateTime LastUpdate { get; set; }
    
    public Building(string id, string type, string builderId, string system, string planet, string resourceCategory, bool isBuilt, DateTime? estimatedBuildTime)
    {
        Id = id;
        Type = type;
        BuilderId = builderId;
        System = system;
        Planet = planet;
        ResourceCategory = resourceCategory;
        IsBuilt = isBuilt;
        EstimatedBuildTime = estimatedBuildTime;
    }
}