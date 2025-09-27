using Backend.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Data;

public interface IAppDbContext
{
    DbSet<User> Users { get; set; }
}