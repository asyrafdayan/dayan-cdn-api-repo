using System.Text.Json.Serialization;

namespace API.Models
{
    public class ResultDTO
    {
        [JsonPropertyName("Result")]
        public object? Result { get; set; }

        [JsonPropertyName("StatusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("Remark")]
        public string? Remark { get; set; }
    }
}
