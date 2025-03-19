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

        public async Task<List<Notification>> GetUnreadNotificationsAsync(int userId){

            IQueryable<Notification> notifications = _context.Notifications
                .AsNoTracking()
                .Take(_limitItem);
                
            var commentList = await notifications.ToListAsync();
            return commentList;
        }

        public async Task<Notification?> GetNotificationByIdAsync(int notificationId)
            => await _context.Notifications
                .AsNoTracking()
                .Where(notification => notification.notificationId == notificationId)
                .FirstOrDefaultAsync();
            
        public async Task<Notification> AddNotificationAsync(Notification notification){
            var newNotification = await _context.AddAsync(notification);
            await _context.SaveChangesAsync();
            return newNotification.Entity;
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
        {
            var affectedRow = await _context.Notifications
                .Where(notification => notification.notificationId == notificationId)
                .ExecuteUpdateAsync(notification => notification
                    .SetProperty(notification => notification.isRead, true));
            return affectedRow == 1;
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
