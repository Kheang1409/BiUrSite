using Backend.Data;
using Backend.Extensions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly int _limitItem;

        public CommentRepository(AppDbContext context, IConfiguration configuration){
            _context = context;
            _configuration = configuration;
            _limitItem = int.Parse(_configuration["Limit"] ?? "10");
        }

        public async Task<List<Comment>> GetCommentsAsync(int pageNumber, string? keyword, int ? userId, int ? postId){

            IQueryable<Comment> comments = _context.Comments
                .AsNoTracking()
                .Include(comment => comment.commenter)
                .Include(comment => comment.post)
                    .ThenInclude(post => post.author);
                
            if(keyword != null)
                comments  = comments.Where(post => EF.Functions.Like(post.description, $"%{keyword}%"));
            if(userId != null)
                comments  = comments.Where(comment => comment.userId == userId);
            if(postId != null)
                comments  = comments.Where(comment => comment.postId == postId);
            comments = comments
                .OrderByDescending(comment => comment.createdDate)
                .Skip(_limitItem*pageNumber)
                .Take(_limitItem);
            var commentList = await comments.FilterAvailableComments().ToListAsync();
            return commentList;
        }

        public async Task<Comment?> GetCommentByIdAsync(int postId, int commentId)
            => await _context.Comments
                .AsNoTracking()
                .Include(comment => comment.commenter)
                .Include(comment => comment.post)
                    .ThenInclude(post => post.author)
                .FilterAvailableComments()
                .FirstOrDefaultAsync();
            
        public async Task<Comment> AddCommentAsync(Comment comment){
            var newComment = await _context.AddAsync(comment);
            await _context.SaveChangesAsync();
            return newComment.Entity;
        }

        public async Task<bool> UpdateContentAsync(int postId, int commentId, string description)
        {
            var affectedRow = await _context.Comments
                .Where(comment => comment.postId == postId)
                .Where(comment => comment.commentId == commentId)
                .FilterAvailableComments()
                .ExecuteUpdateAsync(comment => comment
                    .SetProperty(comment => comment.description, description)
                    .SetProperty(comment => comment.modifiedDate, DateTime.UtcNow));
            return affectedRow == 1;
        }

        public async Task<bool> SoftDeleteCommentAsync(int postId, int commentId)
        {
            var affectedRow = await _context.Comments
                .Where(comment => comment.postId == postId)
                .Where(comment => comment.commentId == commentId)
                .FilterAvailableComments()
                .ExecuteUpdateAsync(comment => comment
                    .SetProperty(comment => comment.isDeleted, true)
                    .SetProperty(comment => comment.deletedDate, DateTime.UtcNow));
            return affectedRow == 1;
        }

        public async Task<bool> DeleteCommentAsync(int postId, int commentId)
        {
            int affectRow = await _context.Comments
                .Where(comment => comment.postId == postId)
                .Where(comment => comment.commentId == commentId)
                .FilterAvailableComments()
                .ExecuteDeleteAsync();
            return affectRow == 1;
        }
    }
}
