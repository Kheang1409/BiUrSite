namespace Tests.TestFixtures;

/// <summary>
/// Base class for all test classes providing common test setup
/// </summary>
public abstract class TestBase : IDisposable
{
    protected TestBase()
    {
        // Common setup for all tests
    }

    public virtual void Dispose()
    {
        // Cleanup after tests
        GC.SuppressFinalize(this);
    }
}
