using Backend.Domain.Enums;
using Backend.Domain.Posts;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend.Domain.Comments;

public class CommentRepository : ICommentRepository
{
    private readonly IMongoCollection<Post> _posts;
    private const string DB_NAME = "bi_ur_site";
    private const string COLLECTION_NAME = "posts";
    private const int LIMIT = 10;
    private const string SUB_DOCUMENT_NAME = "Comments";

    public CommentRepository(
        IMongoClient mongoClient
        )
    {
        var database = mongoClient.GetDatabase(DB_NAME);
        _posts = database.GetCollection<Post>(COLLECTION_NAME);
    }

    public async Task<IEnumerable<Comment>> GetComments(string postId, int pageNumber)
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
            .Select(c => MongoDB.Bson.Serialization.BsonSerializer.Deserialize<Comment>(c.AsBsonDocument));

        return comments;
    }

    public async Task<Comment?> GetCommentById(string postId, string commentId)
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

        return post?.Comments?.FirstOrDefault(c => c.Id == commentId);
    }


    public async Task<Comment> Create(string postId, Comment comment)
    {
        var update = Builders<Post>.Update.Push(SUB_DOCUMENT_NAME, comment);
        await _posts.UpdateOneAsync(
            Builders<Post>.Filter.Eq(p => p.Id, postId),
            update
        );
        return comment;
    }

    public async Task Update(string postId, Comment comment)
    {
        var filter = Builders<Post>.Filter.And(
            Builders<Post>.Filter.Eq(p => p.Id, postId),
            Builders<Post>.Filter.ElemMatch(p => p.Comments, c => c.Id == comment.Id)
        );

        var update = Builders<Post>.Update
            .Set("Comments.$.Text", comment.Text)
            .Set("Comments.$.ModifiedDate", comment.ModifiedDate);

        await _posts.UpdateOneAsync(filter, update);
    }

    public async Task Delete(string postId, Comment comment)
    {
        var filter = Builders<Post>.Filter.And(
            Builders<Post>.Filter.Eq(p => p.Id, postId),
            Builders<Post>.Filter.ElemMatch(p => p.Comments, c => c.Id == comment.Id)
        );

        var update = Builders<Post>.Update
            .Set("Comments.$.Status", comment.Status)
            .Set("Comments.$.DeletedDate", comment.DeletedDate);

        await _posts.UpdateOneAsync(filter, update);
    }

}