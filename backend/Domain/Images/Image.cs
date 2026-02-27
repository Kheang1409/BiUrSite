namespace Backend.Domain.Images;

public record Image
{
    public ImageId Id { get; private set; }
    public string Url { get; private set; }

    public Image(string url)
    {
        Id = new ImageId(Guid.NewGuid());
        Url = url;
    }
}