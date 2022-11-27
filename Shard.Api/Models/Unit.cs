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
    public int Health { get; set; }
    
    [JsonIgnore]public string Owner { get; set; }
    public int Damage { get; set; }
    [JsonIgnore] public List<Weapon> Weapons { get; set; }
    [JsonIgnore] public DateTime LastUpdate { get; set; }
    [JsonIgnore] public Task MoveTask { get; set; }
    [JsonIgnore] public int ETA { get; set; }


    [JsonConstructor]
    public Unit(string id, string type, string system, string planet)
    {
        Id = id;
        Type = type;
        System = system;
        Planet = planet;

        Health = type switch
        {
            "bomber" => 50,
            "cruiser" => 400,
            "fighter" => 80,
            _ => 0
        };
    }

    public Unit(string id, string type, string system, string planet, string owner)
    {
        Id = id;
        Type = type;
        System = system;
        Planet = planet;
        Owner = owner;

        Health = type switch
        {
            "bomber" => 50,
            "cruiser" => 400,
            "fighter" => 80,
            _ => 0
        };
    }

    public void EquipWeapons(IClock clock)
    {
        Weapons = new List<Weapon>();
        switch (Type)
        {
            case "bomber":
                Weapons.Add(new Weapon("bomb", 400, TimeSpan.FromSeconds(60),clock.Now));
                break;
            case "cruiser":
                Weapons.Add(new Weapon("canon", 10, TimeSpan.FromSeconds(6),clock.Now));
                Weapons.Add(new Weapon("canon", 10, TimeSpan.FromSeconds(6),clock.Now));
                Weapons.Add(new Weapon("canon", 10, TimeSpan.FromSeconds(6),clock.Now));
                Weapons.Add(new Weapon("canon", 10, TimeSpan.FromSeconds(6),clock.Now));
                break;
            case "fighter":
                Weapons.Add(new Weapon("canon", 10, TimeSpan.FromSeconds(6),clock.Now));
                break;
        }
    }
}