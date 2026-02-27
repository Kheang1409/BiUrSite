using Backend.Domain.Comments;
using Backend.Domain.Enums;
using Backend.Domain.Posts;
using Backend.Domain.Users;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Resilience;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Backend.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<Post> _posts;
    private readonly ILogger<CommentRepository> _logger;
    private const int LIMIT = 10;
    private const string SUB_DOCUMENT_NAME = "Comments";

    public CommentRepository(MongoDbContext context, ILogger<CommentRepository> logger)
    {
        _posts = context.Posts;
        _users = context.Users;
        _logger = logger;
    }

    public async Task<IEnumerable<Comment>> GetComments(PostId postId, int pageNumber)
    {
        var skip = (pageNumber - 1) * LIMIT;

        var filter = Builders<Post>.Filter.And(
            Builders<Post>.Filter.Eq(p => p.Id, postId),
            Builders<Post>.Filter.Eq(p => p.Status, Status.Active));

        // Get all comments and handle pagination in memory with proper sorting
        var post = await RetryPolicy.ExecuteWithRetryAsync(
            () => _posts.Find(filter)
                .Project<Post>(Builders<Post>.Projection.Include(p => p.Id).Include("Comments"))
                .FirstOrDefaultAsync(),
            logger: _logger,
            operationName: "GetComments");

        if (post?.Comments == null)
            return Enumerable.Empty<Comment>();

        var comments = post.Comments
            .Where(c => c.Status == Status.Active)
            .OrderByDescending(c => c.CreatedDate)
            .Skip(skip)
            .Take(LIMIT)
            .ToList();

        var userIds = comments.Select(c => c.UserId).Distinct().ToList();
        if (userIds.Count > 0)
        {
            var users = await RetryPolicy.ExecuteWithRetryAsync(
                () => _users.Find(Builders<User>.Filter.In(u => u.Id, userIds)).ToListAsync(),
                logger: _logger,
                operationName: "GetComments_FetchUsers");
            
            var userById = users.ToDictionary(u => u.Id, u => u);
            foreach (var comment in comments)
            {
                if (userById.TryGetValue(comment.UserId, out var user))
                {
                    comment.SetUser(user);
                }
            }
        }
        return comments;
    }

    public async Task<Comment?> GetCommentById(PostId postId, CommentId commentId)
    {
        _logger.LogInformation("GetCommentById: PostId={PostId}, CommentId={CommentId}", postId.Value, commentId.Value);
        
        var filter = Builders<Post>.Filter.And(
            Builders<Post>.Filter.Eq(p => p.Id, postId),
            Builders<Post>.Filter.Eq(p => p.Status, Status.Active));

        var projection = Builders<Post>.Projection
            .ElemMatch(SUB_DOCUMENT_NAME, Builders<Comment>.Filter.Eq(c => c.Id, commentId));

        var post = await RetryPolicy.ExecuteWithRetryAsync(
            () => _posts.Find(filter).Project<Post>(projection).SingleOrDefaultAsync(),
            logger: _logger,
            operationName: "GetCommentById");
        
        _logger.LogInformation("GetCommentById: Post found={Found}, CommentsCount={Count}", 
            post != null, post?.Comments?.Count() ?? 0);

        if (post?.Comments == null)
            return null;
            
        var comment = post.Comments.FirstOrDefault(c => c.Id != null && c.Id.Value == commentId.Value);
        
        _logger.LogInformation("GetCommentById: Comment found={Found}, Status={Status}", 
            comment != null, comment?.Status);
        
        if (comment?.Status == Status.Active)
        {
            var user = await RetryPolicy.ExecuteWithRetryAsync(
                () => _users.Find(Builders<User>.Filter.Eq(u => u.Id, comment.UserId)).FirstOrDefaultAsync(),
                logger: _logger,
                operationName: "GetCommentById_FetchUser");
            comment.SetUser(user);
            return comment;
        }
        return null;
    }

    public async Task<Comment> Create(PostId postId, Comment comment)
    {
        var update = Builders<Post>.Update.PushEach(
            SUB_DOCUMENT_NAME,
            new[] { comment },
            position: 0);
        
        await RetryPolicy.ExecuteWithRetryAsync(
            () => _posts.UpdateOneAsync(Builders<Post>.Filter.Eq(p => p.Id, postId), update),
            logger: _logger,
            operationName: "CreateComment");

        return comment;
    }

    public Task Update(PostId postId, Comment comment)
    {
        var filter = Builders<Post>.Filter.And(
            Builders<Post>.Filter.Eq(p => p.Id, postId),
            Builders<Post>.Filter.ElemMatch(SUB_DOCUMENT_NAME, Builders<Comment>.Filter.Eq(c => c.Id, comment.Id)));

        var update = Builders<Post>.Update
            .Set($"{SUB_DOCUMENT_NAME}.$.Text", comment.Text)
            .Set($"{SUB_DOCUMENT_NAME}.$.ModifiedDate", comment.ModifiedDate);

        return RetryPolicy.ExecuteWithRetryAsync(
            () => _posts.UpdateOneAsync(filter, update),
            logger: _logger,
            operationName: "UpdateComment");
    }

    public async Task Delete(PostId postId, Comment comment)
    {
        var filter = Builders<Post>.Filter.And(
            Builders<Post>.Filter.Eq(p => p.Id, postId),
            Builders<Post>.Filter.ElemMatch(SUB_DOCUMENT_NAME, Builders<Comment>.Filter.Eq(c => c.Id, comment.Id)));

        var update = Builders<Post>.Update
            .Set($"{SUB_DOCUMENT_NAME}.$.Status", (int)comment.Status)
            .Set($"{SUB_DOCUMENT_NAME}.$.DeletedDate", comment.DeletedDate);

        await RetryPolicy.ExecuteWithRetryAsync(
            () => _posts.UpdateOneAsync(filter, update),
            logger: _logger,
            operationName: "DeleteComment");
    }
}