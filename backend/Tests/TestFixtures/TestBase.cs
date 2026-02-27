namespace Tests.TestFixtures;
public abstract class TestBase : IDisposable
{
    protected TestBase()
    {
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
