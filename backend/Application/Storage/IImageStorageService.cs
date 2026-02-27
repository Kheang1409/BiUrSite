namespace Backend.Application.Storage;

public interface IImageStorageService
{
    Task<string> UploadImageAsync(string fileName, byte[] content);
    Task DeleteImageAsync(string fileName);
}