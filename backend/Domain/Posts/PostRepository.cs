using Backend.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend.Domain.Posts;

public class PostRepository : IPostRepository
{
    private readonly IMongoCollection<Post> _posts;
    private const string DB_NAME = "bi_ur_site";
    private const string COLLECTION_NAME = "posts";
    private const int LIMIT = 10;

    public PostRepository(
        IMongoClient mongoClient
        )
    {
        var database = mongoClient.GetDatabase(DB_NAME);
        _posts = database.GetCollection<Post>(COLLECTION_NAME);
    }

    public async Task<IEnumerable<Post>> GetPosts(string? username, string? keywords, int pageNumber)
    {
        var filterBuilder = Builders<Post>.Filter;

        var filters = new List<FilterDefinition<Post>>();

        if (!string.IsNullOrWhiteSpace(keywords))
        {
            filters.Add(filterBuilder.Regex(p => p.Text, new BsonRegularExpression(keywords, "i")));
        }
        if (!string.IsNullOrWhiteSpace(username))
        {
            filters.Add(filterBuilder.Regex(u => u.Username, new BsonRegularExpression(keywords, "i")));
        }

        filters.Add(filterBuilder.Where(p => p.Status == Status.Active));

        var finalFilter = filters.Count > 0
            ? filterBuilder.And(filters)
            : FilterDefinition<Post>.Empty;

        var skip = (pageNumber - 1) * LIMIT;

        return await _posts.Find(finalFilter)
                            .SortByDescending(p => p.CreatedDate)
                            .Skip(skip)
                            .Limit(LIMIT)
                            .ToListAsync();
    }

    public async Task<Post?> GetPostById(string id)
    {
        return await _posts
            .Find(p => p.Id == id && p.Status == Status.Active)
            .SingleOrDefaultAsync();
    }

    public async Task<Post> Create(Post post)
    {
        await _posts.InsertOneAsync(post);
        return post;
    }

    public async Task Update(Post post)
    {
        var update = Builders<Post>.Update
            .Set(p => p.Text, post.Text)
            .Set(p => p.Image, post.Image)
            .Set(p => p.ModifiedDate, post.ModifiedDate);
        await _posts.UpdateOneAsync(p => p.Id == post.Id, update);
    }
    
    public async Task Delete(Post post)
    {
        var update = Builders<Post>.Update
            .Set(p => p.Status, post.Status)
            .Set(p => p.Image, null)
            .Set(p => p.DeletedDate, post.DeletedDate);
        await _posts.UpdateOneAsync(p => p.Id == post.Id, update);
    }
}