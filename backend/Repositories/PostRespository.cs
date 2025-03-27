using Backend.Data;
using Backend.Extensions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{

    public class PostRepository : IPostRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly int _limitItem;

        public PostRepository(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _limitItem = int.Parse(_configuration["Limit"] ?? "10");
        }
        
        public async Task<List<Post>> GetPostsAsync(int pageNumber, string? keyword)
        {
            IQueryable<Post> posts = _context.Posts
                .AsNoTracking()
                .Include(post => post.author);
            if(keyword != null){
                posts  = posts.Where(post => EF.Functions.Like(post.description, $"%{keyword}%"));
            }
            posts = posts
                .OrderByDescending(post => post.createdDate)
                .Skip(_limitItem*pageNumber)
                .Take(_limitItem);
            var postList =  await  posts.FilterAvailablePost().ToListAsync();
            return postList;
        }

        public async Task<Post?> GetPostByIdAsync(int postId)
            => await _context.Posts
                .AsNoTracking()
                .Include(post => post.author)
                .Include(post => post.comments)
                .ThenInclude(comment => comment.commenter)
                .Where(post => post.postId == postId)
                .FilterAvailablePost()
                .FirstOrDefaultAsync();

        public async Task<int> GetUserTotalPostAsync(int userId)
            => await _context.Posts
                .AsNoTracking()
                .Where(post => post.author.userId == userId)
                .FilterAvailablePost()
                .CountAsync();

        public async Task<Post> AddPostAsync(Post post)
        {
            var createdPost = await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
            return createdPost.Entity;
        }

        public async Task<bool> UpdateContentAsync(int postId, string description)
        {
            int affectRow = await _context.Posts
                .Where(post => post.postId == postId)
                .FilterAvailablePost()
                .ExecuteUpdateAsync(post => post
                    .SetProperty(post => post.description, description)
                    .SetProperty(post => post.modifiedDate, DateTime.UtcNow));
            return affectRow == 1;
        }
        public async Task<bool> SoftDeletePostAsync(int postId)
        {
            int affectRow = await _context.Posts
            .Where(post=> post.postId == postId)
            .FilterAvailablePost()
            .ExecuteUpdateAsync(post => post
                .SetProperty(post => post.isDeleted, true)
                .SetProperty(post => post.deletedDate, DateTime.UtcNow));
            return affectRow == 1;
        }

        public async Task<bool> DeletePostAsync(int postId)
        {
            int affectRow = await _context.Posts
                .Where(post=> post.postId == postId)
                .ExecuteDeleteAsync();
            return affectRow == 1;
        }
    }
}
