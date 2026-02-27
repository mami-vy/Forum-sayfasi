using Microsoft.EntityFrameworkCore;
using mym.Data;
using mym.Models;

namespace mym.Services;

public class AdminNotificationService : IAdminNotificationService
{
    private readonly ApplicationDbContext _dbContext;

    public AdminNotificationService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateAsync(string title, string message, string type = "info")
    {
        _dbContext.AdminNotifications.Add(new AdminNotification
        {
            Title = title,
            Message = message,
            Type = type,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<AdminNotification>> GetLatestAsync(int take = 20)
    {
        return await _dbContext.AdminNotifications
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(take)
            .ToListAsync();
    }

    public Task<int> GetUnreadCountAsync()
    {
        return _dbContext.AdminNotifications.CountAsync(x => !x.IsRead);
    }

    public async Task MarkAllAsReadAsync()
    {
        var unread = await _dbContext.AdminNotifications
            .Where(x => !x.IsRead)
            .ToListAsync();

        if (!unread.Any())
        {
            return;
        }

        foreach (var item in unread)
        {
            item.IsRead = true;
        }

        await _dbContext.SaveChangesAsync();
    }
}
