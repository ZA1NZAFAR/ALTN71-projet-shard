using Shard.Shared.Core;

namespace Shard.Api.Models;

public class Vaisseau
{
    string Nom { get; set; }
    SystemSpecification SystemSpecification { get; set; }

    public Vaisseau(string nom, SystemSpecification systemSpecification)
    {
        Nom = nom;
        SystemSpecification = systemSpecification;
    }

    public void changeSystemSpecification(SystemSpecification systemSpecification)
    {
        SystemSpecification = systemSpecification;
    }
}