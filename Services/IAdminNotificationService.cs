using mym.Models;

namespace mym.Services;

public interface IAdminNotificationService
{
    Task CreateAsync(string title, string message, string type = "info");
    Task<List<AdminNotification>> GetLatestAsync(int take = 20);
    Task<int> GetUnreadCountAsync();
    Task MarkAllAsReadAsync();
}
