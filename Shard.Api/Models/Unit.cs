using System.Text.Json.Serialization;
using Microsoft.VisualBasic;

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
        EquipWeapons();
    }

    private void EquipWeapons()
    {
        Weapons = new List<Weapon>();
        switch (Type)
        {
            case "bomber":
                Weapons.Add(new Weapon("canon", 10,TimeSpan.FromSeconds(6),DateTime.Now));
                break;
            case "cruiser":
                Weapons.Add(new Weapon("canon", 10,TimeSpan.FromSeconds(6),DateTime.Now));
                Weapons.Add(new Weapon("canon", 10,TimeSpan.FromSeconds(6),DateTime.Now));
                Weapons.Add(new Weapon("canon", 10,TimeSpan.FromSeconds(6),DateTime.Now));
                Weapons.Add(new Weapon("canon", 10,TimeSpan.FromSeconds(6),DateTime.Now));
                break;
            case "fighter":
                Weapons.Add(new Weapon("bomb", 400,TimeSpan.FromSeconds(60),DateTime.Now));
                break;
        }
    }
}