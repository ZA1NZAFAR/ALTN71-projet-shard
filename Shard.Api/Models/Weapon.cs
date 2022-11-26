namespace Shard.Api.Models;

public class Weapon
{
    public string Type { get; set; }
    public int Damage { get; set; }
    public TimeSpan Interval { get; set; }
    public DateTime LastUsed { get; set; }

    public Weapon(string type, int damage, TimeSpan interval, DateTime lastUsed)
    {
        Type = type;
        Damage = damage;
        Interval = interval;
        LastUsed = lastUsed;
    }
}