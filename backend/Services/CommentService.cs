using AutoMapper;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<List<Comment>> GetCommentsAsync(int pageNumber, string? keyword, int? userId, int postId)
            => await _commentRepository.GetCommentsAsync(pageNumber, keyword, userId, postId);

        public async Task<Comment?> GetCommentByIdAsync(int postId, int commentId)
             => await _commentRepository.GetCommentByIdAsync(postId, commentId);
            
        public async Task<Comment> AddCommentAsync(Comment comment)
            => await _commentRepository.AddCommentAsync(comment);

        public async Task<bool> UpdateContentAsync(int postId, int commentId, string description)
            => await _commentRepository.UpdateContentAsync(postId, commentId, description);

        public async Task<bool> SoftDeleteCommentAsync(int postId, int commentId)
            => await _commentRepository.SoftDeleteCommentAsync(postId, commentId);

        public async Task<bool> DeleteCommentAsync(int postId, int commentId)
            => await _commentRepository.DeleteCommentAsync(postId, commentId);
    }
}