namespace Shard.Shared.Core;

public partial class RandomShareComputer
{
    private class Carbon : IResource
    {
        public ResourceKind Kind => ResourceKind.Carbon;
        public double ProbabilityOfPresence => 0.7;
        public int PresenceMultiplier => 5;
    }

    private class Iron : IResource
    {
        public ResourceKind Kind => ResourceKind.Iron;
        public double ProbabilityOfPresence => 0.5;
        public int PresenceMultiplier => 3;
    }

    private class Gold : IResource
    {
        public ResourceKind Kind => ResourceKind.Gold;
        public double ProbabilityOfPresence => 0.1;
        public int PresenceMultiplier => 3;
    }

    private class Aluminium : IResource
    {
        public ResourceKind Kind => ResourceKind.Aluminium;
        public double ProbabilityOfPresence => 0.2;
        public int PresenceMultiplier => 4;
    }

    private class Titanium : IResource
    {
        public ResourceKind Kind => ResourceKind.Titanium;
        public double ProbabilityOfPresence => 0.1;
        public int PresenceMultiplier => 2;
    }

    private class Water : IResource
    {
        public ResourceKind Kind => ResourceKind.Water;
        public double ProbabilityOfPresence => 0.2;
        public int PresenceMultiplier => 1;
    }

    private class Oxygen : IResource
    {
        public ResourceKind Kind => ResourceKind.Oxygen;
        public double ProbabilityOfPresence => 0.4;
        public int PresenceMultiplier => 1;
    }
}
