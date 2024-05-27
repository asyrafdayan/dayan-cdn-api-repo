using API.Interfaces;
using API.Models;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace API.Services
{
    public class RedisService : IRedisService
    {
        private readonly AppSettingsModel? _appSettings;
        private readonly IDatabase? _rdb;
        private readonly ILogger<RedisService> _logger;
        private readonly ConnectionMultiplexer? _redisConnection;

        public RedisService(IOptions<AppSettingsModel> appSettings, ILogger<RedisService> logger)
        {
            _appSettings = appSettings.Value;
            _logger = logger;
            try
            {
                _redisConnection = ConnectionMultiplexer.Connect(_appSettings!.ConnectionStrings!.Redis!);
                _rdb = _redisConnection?.GetDatabase();
            }
            finally
            {
                _logger.LogInformation($"Connected to Redis: {_redisConnection?.IsConnected}");
            }
        }


        public T? GetData<T>(string key)
        {
            if (_redisConnection == null || !_redisConnection.IsConnected)
            {
                _logger.LogError("Not connected to Redis instance");
                return default;
            }

            using (_redisConnection)
            {
                try
                {
                    RedisValue? value = _rdb?.StringGet(key);
                    _logger.LogInformation($"Key: {key}, Value: {value}");
                    if (!string.IsNullOrEmpty(value))
                    {
                        return JsonSerializer.Deserialize<T>(value!);
                    }
                    return default!;
                }
                catch (Exception e)
                {
                    _logger.LogError($"Redis GetData Exception: {e.Message}");
                    return default!;
                }
            }
        }

        public bool? SetData<T>(string key, T data, DateTimeOffset? dateTimeOffset = null)
        {

            if (_redisConnection == null || !_redisConnection.IsConnected)
            {
                _logger.LogError("Not connected to Redis instance");
                return default;
            }

            using (_redisConnection)
            {
                try
                {
                    TimeSpan? expiryTime = dateTimeOffset != null ? dateTimeOffset?.DateTime.Subtract(DateTime.Now) : null;
                    bool? isSet = _rdb?.StringSet(key, JsonSerializer.Serialize(data), expiryTime);
                    _logger.LogInformation($"Key: {key}, Data: {data}, Set: {isSet}");
                    return isSet;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Redis SetData Exception: {ex.Message}");
                    return false;
                }
            }
        }
    }
}
