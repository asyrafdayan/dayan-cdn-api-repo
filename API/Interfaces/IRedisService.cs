namespace API.Interfaces
{
    public interface IRedisService
    {
        void DeleteData(string key);

        T? GetData<T>(string key);

        bool? SetData<T>(string key, T data, DateTimeOffset? dateTimeOffset = null);
    }
}
