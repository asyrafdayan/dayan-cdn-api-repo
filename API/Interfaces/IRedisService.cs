namespace API.Interfaces
{
    public interface IRedisService
    {
        T? GetData<T>(string key);

        bool? SetData<T>(string key, T data, DateTimeOffset? dateTimeOffset = null);
    }
}
