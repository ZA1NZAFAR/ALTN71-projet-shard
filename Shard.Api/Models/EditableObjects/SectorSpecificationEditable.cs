using Shard.Shared.Core;

namespace Shard.Api.Models.EditableObjects;

public class SectorSpecificationEditable
{
    public List<SystemSpecificationEditable> Systems { get; }

    public SectorSpecificationEditable(List<SystemSpecificationEditable> systems)
    {
        Systems = systems;
    }
}
