using MiNet.Data.Models;

namespace MiNet.Data.Services
{
    public interface INotificationsService
    {
        Task AddNewNotificationAsync(int userId, string notificationType, string userName, int? postId);
        Task<int> GetUnreadNotificationsCountAsync(int userId);
        Task<List<Notification>> GetNotifications(int userId);
        Task SetNotificationAsReadAsync(int notificationId);
    }
}
