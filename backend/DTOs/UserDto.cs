namespace Backend.DTOs
{
    public class UserDto
    {
        public int userId { get; set; }
        
        public string username { get; set; }

        public string email { get; set; }

        public bool isActive { get; set; }

        public string status { get; set; }

        public DateTime? createdDate { get; set; }
    }
}
