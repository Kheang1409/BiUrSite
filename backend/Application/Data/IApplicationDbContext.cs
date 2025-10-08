using Backend.Domain.Posts;
using Backend.Domain.Users;
using MongoDB.Driver;

namespace Backend.Application.Data;

public interface IAppDbContext
{
    IMongoCollection<User> Users { get;}
    IMongoCollection<Post> Posts  { get;}
}