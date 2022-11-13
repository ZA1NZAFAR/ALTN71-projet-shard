using System.Text.Json.Serialization;
using Shard.Shared.Core;

namespace Shard.Api.Models;

public class Unit
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string System { get; set; }
    public string Planet { get; set; }

    public string DestinationPlanet { get; set; }
    public string DestinationSystem { get; set; }

    public DateTime LastUpdate { get; set; }
    public Task MoveTask { get; set; }
    public int ETA { get; set; }


    SystemSpecification SystemSpecification { get; set; }
    

    [JsonConstructor]
    public Unit(string id, string type, string system, string planet)
    {
        this.Id = id;
        this.Type = type;
        this.System = system;
        this.Planet = planet;
    }
}