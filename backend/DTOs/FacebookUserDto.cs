namespace Backend.Models
{
    public class FacebookUserDto
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public FacebookPictureDto picture { get; set; }
    }

    public class FacebookPictureDto
    {
        public FacebookPictureDataDto data { get; set; }
    }

    public class FacebookPictureDataDto
    {
        public string url { get; set; }
    }
}
