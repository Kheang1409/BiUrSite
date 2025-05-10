using Backend.Domain.Comments.Entities;
using Backend.Domain.Comments.Interfaces;
using Backend.Infrastructure.Extensions;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Backend.Infrastructure.Repositories;
public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly int _limitItem;

    public CommentRepository(AppDbContext context, IConfiguration configuration){
        _context = context;
        _configuration = configuration;
        _limitItem = int.Parse(_configuration["LimitSettings:ItemLimit"] ?? "10");
    }

    public async Task<List<Comment>> GetCommentsAsync(int postId, int pageNumber){

        IQueryable<Comment> comments =  _context.Comments
            .AsNoTracking()
            .Where(comment => comment.PostId == postId)
            .Include(comment => comment.Commenter)
            .OrderByDescending(comment => comment.CreatedDate)
            .Skip(_limitItem*(pageNumber-1))
            .Take(_limitItem);
        var commentList =  await comments.FilterAvailableComments().ToListAsync();
        return commentList;
    }

    public async Task<Comment?> GetCommentByIdAsync(int commentId)
        => await _context.Comments
            .AsNoTracking()
            .Where(comment => comment.Id == commentId)
            .Include(comment => comment.Commenter)
            .FilterAvailableComments()
            .FirstOrDefaultAsync();
        
    public async Task<Comment> CreateCommentAsync(Comment comment){
        var newComment = await _context.AddAsync(comment);
        await _context.SaveChangesAsync();
        return newComment.Entity;
    }

    public async Task<bool> UpdateCommentAsync(int commentId, string description)
    {
        var affectedRow = await _context.Comments
            .Where(comment => comment.Id == commentId)
            .FilterAvailableComments()
            .ExecuteUpdateAsync(comment => comment
                .SetProperty(comment => comment.Description, description)
                .SetProperty(comment => comment.ModifiedDate, DateTime.UtcNow));
        return affectedRow == 1;
    }

    public async Task<bool> SoftDeleteCommentAsync(int commentId)
    {
        var affectedRow = await _context.Comments
            .Where(comment => comment.Id == commentId)
            .FilterAvailableComments()
            .ExecuteUpdateAsync(comment => comment
                .SetProperty(comment => comment.IsDeleted, true)
                .SetProperty(comment => comment.DeletedDate, DateTime.UtcNow));
        return affectedRow == 1;
    }

    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        int affectRow = await _context.Comments
            .Where(comment => comment.Id == commentId)
            .FilterAvailableComments()
            .ExecuteDeleteAsync();
        return affectRow == 1;
    }
}