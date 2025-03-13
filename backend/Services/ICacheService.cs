namespace Backend.Services
{
    public interface ICacheService
    {

        Task SetDataAsync<T>(string key, T value, TimeSpan expiry);

        Task<T?> GetDataAsync<T>(string key);

        Task RemoveDataAsync(string key);
    }
}