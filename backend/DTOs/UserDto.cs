using Backend.Enums;
using System.Text.Json.Serialization;

namespace Backend.DTOs
{
    public class UserDto
    {
        public int userId { get; set; }
        public required string username { get; set; }
        public required string email { get; set; }
        public string? profile {get; set; }
        public bool isActive { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Status status { get; set; }
        public DateTime? createdDate { get; set; }
        public DateTime? modifiedDate {get; set; }
    }
}
