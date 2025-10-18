using Backend.Domain.Enums;
using Backend.Domain.Posts;
using Backend.Domain.Users;
using Backend.Infrastructure.Persistence;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<Post> _posts;
    private const string SUB_DOCUMENT_NAME = "Comments";

    private const int LIMIT = 10;

    public PostRepository(
        MongoDbContext context
        )
    {
        _posts = context.Posts;
        _users = context.Users;
    }

    public async Task<IEnumerable<Post>> GetPosts(UserId? userId, string? keywords, int pageNumber)
    {
        var filterBuilder = Builders<Post>.Filter;

        var filters = new List<FilterDefinition<Post>>();

        if (!string.IsNullOrWhiteSpace(keywords))
        {
            filters.Add(filterBuilder.Regex(p => p.Text, new BsonRegularExpression(keywords, "i")));
        }
        if (userId is not null)
        {
            filters.Add(filterBuilder.Eq(p => p.UserId, userId));
        }

        filters.Add(filterBuilder.Where(p => p.Status == Status.Active));

        var finalFilter = filters.Count > 0
            ? filterBuilder.And(filters)
            : FilterDefinition<Post>.Empty;

        var skip = (pageNumber - 1) * LIMIT;

        var posts = await _posts.Find(finalFilter)
                            .Project<Post>(Builders<Post>.Projection
                                .Exclude(SUB_DOCUMENT_NAME))
                            .SortByDescending(p => p.CreatedDate)
                            .Skip(skip)
                            .Limit(LIMIT)
                            .ToListAsync();

        foreach (var post in posts)
        {
            var user = await _users.Find(Builders<User>.Filter.Eq(u => u.Id, post.UserId)).FirstOrDefaultAsync();
            post.SetUser(user);
        }
        return posts;
    }

    public async Task<Post?> GetPostById(PostId id)
    {
        var filter = Builders<Post>.Filter.And(
            Builders<Post>.Filter.Eq(p => p.Id, id),
            Builders<Post>.Filter.Eq(p => p.Status, Status.Active)
        );
        var projection = Builders<Post>.Projection
            .Slice(SUB_DOCUMENT_NAME, -LIMIT/2);
        var post = await _posts
                            .Find(filter)
                            .Project<Post>(projection)
                            .SingleOrDefaultAsync();
        if (post == null)
            return null;
        var user = await _users.Find(Builders<User>.Filter.Eq(u => u.Id, post.UserId)).FirstOrDefaultAsync();
        post.SetUser(user);
        var sortedComments = post.Comments.OrderByDescending(c => c.CreatedDate).ToList();
        var commentsField = typeof(Post).GetField("_comments", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        commentsField?.SetValue(post, sortedComments);
        return post;
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