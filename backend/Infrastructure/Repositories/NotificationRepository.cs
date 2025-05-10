using Backend.Domain.Notifications.Entities;
using Backend.Domain.Notifications.Interfaces;
using Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Backend.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;
    private readonly int _limitItem;

    public NotificationRepository(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _limitItem = int.TryParse(configuration["LimitSettings:ItemLimit"], out var parsedLimit) 
                     ? parsedLimit / 2 
                     : 5;
    }

    public async Task<List<Notification>> GetNotificationsByUserIdAsync(int userId)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedDate);
        return await query.Take(_limitItem).ToListAsync();
    }

    public async Task<Notification?> GetNotificationByIdAsync(int notificationId) =>
        await _context.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(n =>
                n.NotificationId == notificationId &&
                !n.IsDeleted);

    public async Task<Notification> CreateNotificationAsync(Notification notification)
    {
        var result = await _context.AddAsync(notification);
        await _context.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<bool> UpdateNotificationReadStatusAsync(int notificationId, bool isRead)
    {
        var affectedRows = await _context.Notifications
            .Where(n => n.NotificationId == notificationId)
            .ExecuteUpdateAsync(setters => 
                setters.SetProperty(n => n.IsRead, isRead));

        return affectedRows == 1;
    }

    public async Task<bool> SoftDeleteNotificationAsync(int notificationId)
    {
        var affectedRows = await _context.Notifications
            .Where(n => n.NotificationId == notificationId)
            .ExecuteUpdateAsync(setters => 
                setters.SetProperty(n => n.IsDeleted, true));

        return affectedRows == 1;
    }

    public async Task<bool> DeleteNotificationAsync(int notificationId)
    {
        var affectedRows = await _context.Notifications
            .Where(n => n.NotificationId == notificationId)
            .ExecuteDeleteAsync();

        return affectedRows == 1;
    }
}