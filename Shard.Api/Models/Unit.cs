using Shard.Shared.Core;
using System.Text.Json.Serialization;

namespace Shard.Api.Models;

public class Vaisseau
{
    public string id { get; set; }
    public string type { get; set; }
    public string system { get; set; }
    public string planet { get; set; }

    public string destinationPlanet { get; set; }
    public string destinationSystem { get; set; }

    public DateTime lastUpdate { get; set; }


    SystemSpecification SystemSpecification { get; set; }
    

    [JsonConstructor]
    public Vaisseau(string id, string type, string system, string planet)
    {
        this.id = id;
        this.type = type;
        this.system = system;
        this.planet = planet;
    }

    public void changeSystemSpecification(SystemSpecification systemSpecification)
    {
        SystemSpecification = systemSpecification;
    }

    public string toString()
    {
        return "Vaisseau " + id + " de type " + type + " dans le système " + system + " sur la planète " + planet;
    }
}