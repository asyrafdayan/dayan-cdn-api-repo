using System.Text.Json.Serialization;

namespace API.Models
{
    public class AppSettingsModel
    {
        [JsonPropertyName("ConnectionStrings")]
        public ConnectionStrings? ConnectionStrings { get; set; }
    }

    public class ConnectionStrings
    {
        [JsonPropertyName("SQLite")]
        public string? SQLite { get; set; }
        [JsonPropertyName("Redis")]
        public string? Redis { get; set; }
    }
}
