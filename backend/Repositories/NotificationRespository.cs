using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly int _limitItem;

        public NotificationRepository(AppDbContext context, IConfiguration configuration){
            _context = context;
            _configuration = configuration;
            _limitItem = int.Parse(_configuration["Limit"] ?? "10")/2;
        }

        public async Task<List<Notification>> GetNotificationsByUserIdAsync(int userId, bool? isRead){

            IQueryable<Notification> notifications = _context.Notifications
                .AsNoTracking()
                .OrderByDescending(notification => notification.createdDate)
                .Where(notification => notification.userId == userId)
                .Where(notification => notification.isDeleted == false);
            if(isRead != null )
                notifications = notifications.Where(notification => notification.isRead == isRead);
            notifications = notifications.Take(_limitItem);
            var notificationList = await notifications.ToListAsync();
            return notificationList;
        }

        public async Task<Notification?> GetNotificationByIdAsync(int notificationId)
            => await _context.Notifications
                .AsNoTracking()
                .Where(notification => notification.notificationId == notificationId)
                .Where(notification => notification.isDeleted == false)
                .FirstOrDefaultAsync();
            
        public async Task<Notification> AddNotificationAsync(Notification notification){
            var newNotification = await _context.AddAsync(notification);
            await _context.SaveChangesAsync();
            return newNotification.Entity;
        }

        public async Task<bool> MarkNotificationsync(int notificationId, bool isRead)
        {
            var affectedRow = await _context.Notifications
                .Where(notification => notification.notificationId == notificationId)
                .ExecuteUpdateAsync(notification => notification
                    .SetProperty(notification => notification.isRead, isRead));
            return affectedRow == 1;
        }

        public async Task<bool> SoftDeleteNotificationAsync(int notificationId)
        {
            int affectRow = await _context.Notifications
                .Where(notification => notification.notificationId == notificationId)
                .ExecuteUpdateAsync(notification => notification
                    .SetProperty(notification => notification.isDeleted, true));
            return affectRow == 1;
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId)
        {
            int affectRow = await _context.Notifications
                .Where(notification => notification.notificationId == notificationId)
                .ExecuteDeleteAsync();
            return affectRow == 1;
        }
    }
}
