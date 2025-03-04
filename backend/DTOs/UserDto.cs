using Backend.Enums;
using Backend.Models;
using System.Text.Json.Serialization;

namespace Backend.DTOs
{
    public class UserDto
    {
        public int userId { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public bool isActive { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Status status { get; set; }
        public DateTime createdDate { get; set; }
    }
}
