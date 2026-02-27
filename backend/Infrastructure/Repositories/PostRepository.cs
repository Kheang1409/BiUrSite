using Backend.Domain.Enums;
using Backend.Domain.Posts;
using Backend.Domain.Users;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Resilience;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<Post> _posts;
    private readonly ILogger<PostRepository> _logger;
    private const string SUB_DOCUMENT_NAME = "Comments";
    private const int LIMIT = 10;

    public PostRepository(MongoDbContext context, ILogger<PostRepository> logger)
    {
        _posts = context.Posts;
        _users = context.Users;
        _logger = logger;
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

        var finalFilter = filterBuilder.And(filters);
        var skip = (pageNumber - 1) * LIMIT;

        var posts = await RetryPolicy.ExecuteWithRetryAsync(
            () => _posts.Find(finalFilter)
                .Project<Post>(Builders<Post>.Projection.Exclude(SUB_DOCUMENT_NAME))
                .SortByDescending(p => p.CreatedDate)
                .Skip(skip)
                .Limit(LIMIT)
                .ToListAsync(),
            logger: _logger,
            operationName: "GetPosts");

        if (posts.Count > 0)
        {
            var userIds = posts.Select(p => p.UserId).Distinct().ToList();
            var users = await RetryPolicy.ExecuteWithRetryAsync(
                () => _users.Find(Builders<User>.Filter.In(u => u.Id, userIds)).ToListAsync(),
                logger: _logger,
                operationName: "GetPosts_FetchUsers");
            
            var userById = users.ToDictionary(u => u.Id, u => u);
            foreach (var post in posts)
            {
                if (userById.TryGetValue(post.UserId, out var user))
                {
                    post.SetUser(user);
                }
            }
        }
        
        return posts;
    }

    public async Task<Post?> GetPostById(PostId id)
    {
        var filter = Builders<Post>.Filter.And(
            Builders<Post>.Filter.Eq(p => p.Id, id),
            Builders<Post>.Filter.Eq(p => p.Status, Status.Active));
        
        // Fetch all comments to get accurate count
        var post = await RetryPolicy.ExecuteWithRetryAsync(
            () => _posts.Find(filter).SingleOrDefaultAsync(),
            logger: _logger,
            operationName: "GetPostById");
        
        if (post == null)
            return null;
        
        var user = await RetryPolicy.ExecuteWithRetryAsync(
            () => _users.Find(Builders<User>.Filter.Eq(u => u.Id, post.UserId)).FirstOrDefaultAsync(),
            logger: _logger,
            operationName: "GetPostById_FetchUser");
        post.SetUser(user);

        var commentUserIds = post.Comments.Select(c => c.UserId).Distinct().ToList();

        if (commentUserIds.Count > 0)
        {
            var commentUsers = await RetryPolicy.ExecuteWithRetryAsync(
                () => _users.Find(Builders<User>.Filter.In(u => u.Id, commentUserIds)).ToListAsync(),
                logger: _logger,
                operationName: "GetPostById_FetchCommentUsers");

            var userById = commentUsers.ToDictionary(u => u.Id, u => u);
            foreach (var comment in post.Comments)
            {
                if (userById.TryGetValue(comment.UserId, out var commentUser))
                {
                    comment.SetUser(commentUser);
                }
            }
        }

        var sortedComments = post.Comments.Where(c => c.Status == Status.Active).OrderByDescending(c => c.CreatedDate).ToList();
        var commentsField = typeof(Post).GetField("_comments", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        commentsField?.SetValue(post, sortedComments);
        return post;
    }

    public async Task<Post> Create(Post post)
    {
        await RetryPolicy.ExecuteWithRetryAsync(
            () => _posts.InsertOneAsync(post),
            logger: _logger,
            operationName: "CreatePost");
        return post;
    }

    public Task Update(Post post)
    {
        var update = Builders<Post>.Update
            .Set(p => p.Text, post.Text)
            .Set(p => p.Image, post.Image)
            .Set(p => p.ModifiedDate, post.ModifiedDate);
        
        return RetryPolicy.ExecuteWithRetryAsync(
            () => _posts.UpdateOneAsync(p => p.Id == post.Id, update),
            logger: _logger,
            operationName: "UpdatePost");
    }
    
    public Task Delete(Post post)
    {
        var update = Builders<Post>.Update
            .Set(p => p.Status, post.Status)
            .Set(p => p.Image, null)
            .Set(p => p.DeletedDate, post.DeletedDate);
        
        return RetryPolicy.ExecuteWithRetryAsync(
            () => _posts.UpdateOneAsync(p => p.Id == post.Id, update),
            logger: _logger,
            operationName: "DeletePost");
    }
}