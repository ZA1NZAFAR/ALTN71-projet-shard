using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Shard.Shared.Core;

public class MapGenerator
{
    private readonly string? seed;

    public static MapGenerator Random => new(new MapGeneratorOptions());

    public MapGenerator(IOptions<MapGeneratorOptions> options)
        : this(options.Value)
    {
    }

    public MapGenerator(MapGeneratorOptions options)
    {
        seed = options.Seed;
    }

    private static int CreateIntegerSeed(string seed)
    {
        using SHA256 sha256Hash = SHA256.Create();

        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(seed));
        return BitConverter.ToInt32(bytes);
    }

    private Random CreateRandom()
        => seed == null ? new Random() : new Random(CreateIntegerSeed(seed));

    public SectorSpecification Generate() 
        => new(CreateRandom());
}
