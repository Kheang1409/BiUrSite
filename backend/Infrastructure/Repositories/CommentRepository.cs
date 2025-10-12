using Backend.Domain.Comments;
using Backend.Domain.Enums;
using Backend.Domain.Posts;
using Backend.Infrastructure.Persistence;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Backend.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly IMongoCollection<Post> _posts;
    private const int LIMIT = 10;
    private const string SUB_DOCUMENT_NAME = "Comments";

    public CommentRepository(
        MongoDbContext context
        )
    {
        _posts = context.Posts;
    }

    public async Task<IEnumerable<Comment>> GetComments(PostId postId, int pageNumber)
    {
        var skip = (pageNumber - 1) * LIMIT;

        var filter = Builders<Post>.Filter.And(
            Builders<Post>.Filter.Eq(p => p.Id, postId),
            Builders<Post>.Filter.Eq(p => p.Status, Status.Active)
        );

        var projection = Builders<Post>.Projection
            .Slice(SUB_DOCUMENT_NAME, skip, LIMIT)
            .Include(p => p.Id);

        var bsonResult = await _posts
            .Find(filter)
            .Project<BsonDocument>(projection)
            .FirstOrDefaultAsync();

        if (bsonResult == null)
            return Enumerable.Empty<Comment>();

        var comments = bsonResult[SUB_DOCUMENT_NAME]
            .AsBsonArray
            .Select(c => BsonSerializer.Deserialize<Comment>(c.AsBsonDocument))
            .Where(c => c.Status == Status.Active);

        return comments;
    }

    public async Task<Comment?> GetCommentById(PostId postId, CommentId commentId)
    {
        var filter = Builders<Post>.Filter.And(
            Builders<Post>.Filter.Eq(p => p.Id, postId),
            Builders<Post>.Filter.Eq(p => p.Status, Status.Active),
            Builders<Post>.Filter.ElemMatch(SUB_DOCUMENT_NAME, Builders<Comment>.Filter.Eq(c => c.Id, commentId))
        );

        var projection = Builders<Post>.Projection
            .ElemMatch(SUB_DOCUMENT_NAME, Builders<Comment>.Filter.Eq(c => c.Id, commentId));

        var post = await _posts.Find(filter)
            .Project<Post>(projection)
            .SingleOrDefaultAsync();

        var comment = post?.Comments?.FirstOrDefault(c => c.Id == commentId);
        return comment?.Status == Status.Active ? comment : null;
    }


    public async Task<Comment> Create(PostId postId, Comment comment)
    {
        var update = Builders<Post>.Update.Push(SUB_DOCUMENT_NAME, comment);
        await _posts.UpdateOneAsync(
            Builders<Post>.Filter.Eq(p => p.Id, postId),
            update
        );
        return comment;
    }

    public async Task Update(PostId postId, Comment comment)
    {
        var filter = Builders<Post>.Filter.And(
            Builders<Post>.Filter.Eq(p => p.Id, postId),
            Builders<Post>.Filter.ElemMatch(SUB_DOCUMENT_NAME, Builders<Comment>.Filter.Eq("Id", comment.Id))
        );

        var update = Builders<Post>.Update
            .Set($"{SUB_DOCUMENT_NAME}.$.Text", comment.Text)
            .Set($"{SUB_DOCUMENT_NAME}.$.ModifiedDate", comment.ModifiedDate);

        await _posts.UpdateOneAsync(filter, update);
    }

    public async Task Delete(PostId postId, Comment comment)
    {
        var filter = Builders<Post>.Filter.And(
            Builders<Post>.Filter.Eq(p => p.Id, postId),
            Builders<Post>.Filter.ElemMatch(SUB_DOCUMENT_NAME, Builders<Comment>.Filter.Eq("Id", comment.Id))
        );

        var update = Builders<Post>.Update
            .Set($"{SUB_DOCUMENT_NAME}.$.Status", comment.Status)
            .Set($"{SUB_DOCUMENT_NAME}.$.DeletedDate", comment.DeletedDate);

        await _posts.UpdateOneAsync(filter, update);
    }

}