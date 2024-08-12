namespace BuildingBlocks.Abstractions.Scheduler;

public class ScheduleSerializedObject
{
    public ScheduleSerializedObject(string fullTypeName, string data, string additionalDescription, string assemblyName)
    {
        FullTypeName = fullTypeName;
        Data = data;
        AdditionalDescription = additionalDescription;
        AssemblyName = assemblyName;
    }

    public string FullTypeName { get; }
    public string Data { get; private set; }
    public string AdditionalDescription { get; }
    public string AssemblyName { get; private set; }

    public override string ToString()
    {
        var commandName = FullTypeName.Split('.').Last();
        return $"{commandName} {AdditionalDescription}";
    }
}
