using IdGen;

namespace BuildingBlocks.Core.IdsGenerator;

public static class SnowFlakIdGenerator
{
    private static readonly IdGenerator _generator;

    static SnowFlakIdGenerator()
    {
        // Read `GENERATOR_ID` from .env file in service root folder or system environment variables
        var generatorId = DotNetEnv.Env.GetInt("GENERATOR_ID", 0);

        // Let's say we take jan 17st 2022 as our epoch
        var epoch = new DateTime(2022, 1, 17, 0, 0, 0, DateTimeKind.Local);

        // Create an ID with 45 bits for timestamp, 2 for generator-id
        // and 16 for sequence
        var structure = new IdStructure(45, 2, 16);

        // Prepare options
        var options = new IdGeneratorOptions(structure, new DefaultTimeSource(epoch));

        // Create an IdGenerator with it's generator-id set to 0, our custom epoch
        // and id-structure
        _generator = new IdGenerator(generatorId, options);
    }

    public static long NewId()
    {
        return _generator.CreateId();
    }
}
