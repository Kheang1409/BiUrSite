using AutoMapper;
using Backend.DTOs;
using Backend.Models;

namespace Backend.AutoMapper
{
    public class PostProfile : Profile
    {
        public PostProfile()
        {
            CreateMap<Post, CreatePostDto>().ReverseMap();
            CreateMap<Post, PostDto>().ReverseMap();
        }
    }
}
