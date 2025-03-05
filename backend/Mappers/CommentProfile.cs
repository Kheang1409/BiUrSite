using AutoMapper;
using Backend.DTOs;
using Backend.Models;

namespace Backend.AutoMapper
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<Comment, CreateCommentDto>().ReverseMap();
            CreateMap<Comment, CommentDto>().ReverseMap();
            
        }
    }
}
