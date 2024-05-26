using System.Text.Json.Serialization;

namespace API.Models
{
    public class FreelancerModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("UserName")]
        public string? Username { get; set; }
        [JsonPropertyName("Email")]
        public string? Email { get; set; }
        [JsonPropertyName("PhoneNumber")]
        public string? PhoneNumber { get; set; }
        [JsonPropertyName("SkillSets")]
        public List<string> SkillSets { get; set; } = [];
        [JsonPropertyName("Hobby")]
        public string? Hobby { get; set; }
    }
}
