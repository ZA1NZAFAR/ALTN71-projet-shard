using System.Text.Json.Serialization;

namespace Shard.Api.Models;

public class UnitType
{
    public string Type { get; set; }
    
    
    [JsonConstructor]
    public UnitType(string type)
    {
        Type = type;
    }
}