namespace Shard.Shared.Core;

public class SystemSpecificationEditable
{
    public string Name { get; }
    public List<PlanetSpecificationEditable> Planets { get; }

    internal SystemSpecificationEditable(string name, List<PlanetSpecificationEditable> planets)
    {
        Name = name;
        Planets = planets;
    }
}
