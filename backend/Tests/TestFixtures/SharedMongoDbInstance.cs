using Mongo2Go;
using MongoDB.Driver;

namespace Tests.TestFixtures;

public static class SharedMongoDbInstance
{
    private static MongoDbRunner? _runner;
    private static readonly object _lock = new();
    private static int _refCount;

    public static string ConnectionString
    {
        get
        {
            EnsureStarted();
            return _runner!.ConnectionString;
        }
    }

    public static void EnsureStarted()
    {
        lock (_lock)
        {
            if (_runner == null)
            {
                _runner = MongoDbRunner.Start();
            }
            _refCount++;
        }
    }

    public static async Task WaitForReadyAsync()
    {
        EnsureStarted();
        
        var client = new MongoClient(_runner!.ConnectionString);
        var maxAttempts = 60;
        var delay = TimeSpan.FromMilliseconds(200);

        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                var databases = await client.ListDatabaseNamesAsync();
                await databases.ToListAsync();
                return;
            }
            catch
            {
                await Task.Delay(delay);
            }
        }

        throw new InvalidOperationException("MongoDB failed to become ready within timeout");
    }

    public static void Release()
    {
        lock (_lock)
        {
            _refCount--;
            if (_refCount <= 0 && _runner != null)
            {
                _runner.Dispose();
                _runner = null;
                _refCount = 0;
            }
        }
    }
}
