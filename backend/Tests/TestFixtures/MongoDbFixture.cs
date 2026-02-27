using Backend.Infrastructure.Configurations;
using Backend.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;

namespace Tests.TestFixtures;

public sealed class MongoDbFixture : IAsyncLifetime
{
    public MongoDbContext Context { get; private set; } = null!;
    public string ConnectionString => SharedMongoDbInstance.ConnectionString;

    public async Task InitializeAsync()
    {
        StronglyTypedIdSerializationRegistry.Register();

        await SharedMongoDbInstance.WaitForReadyAsync();

        var inMemorySettings = new Dictionary<string, string?>
        {
            { "MongoDB:ConnectionString", SharedMongoDbInstance.ConnectionString },
            { "MongoDB:Name", "IntegrationTestsDb" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        Context = new MongoDbContext(configuration, null);
    }

    public Task DisposeAsync()
    {
        SharedMongoDbInstance.Release();
        return Task.CompletedTask;
    }
}

[CollectionDefinition("MongoDB")]
public class MongoDbCollection : ICollectionFixture<MongoDbFixture>
{
}
