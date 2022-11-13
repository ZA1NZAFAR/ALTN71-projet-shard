namespace Shard.Shared.Core;

public class SectorSpecificationEditable
{
    public List<SystemSpecificationEditable> Systems { get; }

    public SectorSpecificationEditable(List<SystemSpecificationEditable> systems)
    {
        Systems = systems;
    }
}
