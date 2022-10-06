using System.Text.Json.Serialization;

namespace Shard.Api.Models;

public class User
{
    public string id { get; set; }
    public string pseudo { get; set; }
    public DateTime dateOfCreation { get; set; }

    [JsonConstructor]
    public User(string id, string pseudo)
    {
        this.id = id;
        this.pseudo = pseudo;
        this.dateOfCreation = DateTime.Now;
    }
}