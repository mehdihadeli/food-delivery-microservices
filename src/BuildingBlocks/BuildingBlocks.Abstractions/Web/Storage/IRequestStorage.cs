namespace BuildingBlocks.Abstractions.Web.Storage;

public interface IRequestStorage
{
    void Set<T>(string key, T value)
        where T : notnull;
    T? Get<T>(string key);
}
