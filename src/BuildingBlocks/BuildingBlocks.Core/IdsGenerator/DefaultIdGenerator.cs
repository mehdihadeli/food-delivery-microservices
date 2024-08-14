using Bogus;
using IdGen;

namespace BuildingBlocks.Core.IdsGenerator;

public class SnowFlakIdGenerator : BuildingBlocks.Abstractions.Core.IIdGenerator<long>
{
    public long New()
    {
        return NewId();
    }

    public static long NewId()
    {
        return _generator.CreateId();
    }

    private static readonly IdGenerator _generator = new(new Faker().Random.Number(0, 3), GetOptions());

    public static IdGeneratorOptions GetOptions()
    {
        // Let's say we take jan 17st 2022 as our epoch
        var epoch = new DateTime(2022, 1, 17, 0, 0, 0, DateTimeKind.Local);

        // Create an ID with 45 bits for timestamp, 2 for generator-id
        // and 16 for sequence
        var structure = new IdStructure(45, 2, 16);

        // Prepare options
        var options = new IdGeneratorOptions(structure, new DefaultTimeSource(epoch));

        return options;
    }
}
