using Backend.Domain.Posts.Entities;
using Backend.Domain.Posts.Interfaces;
using Backend.Infrastructure.Extensions;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Backend.Infrastructure.Repositories;
public class PostRepository : IPostRepository
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly int _limitItem;

    public PostRepository(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _limitItem = int.Parse(_configuration["LimitSettings:ItemLimit"] ?? "10");
    }
    
    public async Task<List<Post>> SearchPostsAsync(string ? keyword, int pageNumber)
    {
        IQueryable<Post> posts = _context.Posts
            .AsNoTracking()
            .Include(post => post.Author);
        if(!string.IsNullOrEmpty(keyword)){
            posts  = posts.Where(post => EF.Functions.Like(post.Description, $"%{keyword}%"));
        }
        posts = posts
            .OrderByDescending(post => post.CreatedDate)
            .Skip(_limitItem*(pageNumber-1))
            .Take(_limitItem);
        var postList =  await  posts.FilterAvailablePost().ToListAsync();
        return postList;
    }

    public async Task<Post?> GetPostByIdAsync(int postId)
        => await _context.Posts
            .AsNoTracking()
            .Include(post => post.Author)
            .Where(post => post.PostId == postId)
            .FilterAvailablePost()
            .FirstOrDefaultAsync();

    public async Task<int> GetUserPostCountAsync(int userId)
        => await _context.Posts
            .AsNoTracking()
            .Where(post => post.Author != null && post.Author.Id == userId)
            .FilterAvailablePost()
            .CountAsync();

    public async Task<Post> CreatePostAsync(Post post)
    {
        var createdPost = await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();
        return createdPost.Entity;
    }

    public async Task<bool> UpdatePostDescriptionAsync(int postId, string description)
    {
        int affectRow = await _context.Posts
            .Where(post => post.PostId == postId)
            .FilterAvailablePost()
            .ExecuteUpdateAsync(post => post
                .SetProperty(post => post.Description, description)
                .SetProperty(post => post.ModifiedDate, DateTime.UtcNow));
        return affectRow == 1;
    }
    public async Task<bool> SoftDeletePostAsync(int postId)
    {
        int affectRow = await _context.Posts
        .Where(post=> post.PostId == postId)
        .FilterAvailablePost()
        .ExecuteUpdateAsync(post => post
            .SetProperty(post => post.IsDeleted, true)
            .SetProperty(post => post.DeletedDate, DateTime.UtcNow));
        return affectRow == 1;
    }

    public async Task<bool> DeletePostAsync(int postId)
    {
        int affectRow = await _context.Posts
            .Where(post=> post.PostId == postId)
            .ExecuteDeleteAsync();
        return affectRow == 1;
    }
}