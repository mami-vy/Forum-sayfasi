using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mym.Data;
using mym.Models;
using mym.Models.Admin;

namespace mym.Controllers;

[Authorize(Policy = "ViewActivity")]
[AutoValidateAntiforgeryToken]
public class AdminActivityController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public AdminActivityController(ApplicationDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var activeWindow = DateTime.UtcNow.AddMinutes(-30);

        // count active unique users by user id or username
        var activeCount = await _db.LoginActivities
            .Where(a => a.IsSuccess && a.CreatedAtUtc >= activeWindow)
            .Select(a => a.UserId ?? a.Username)
            .Distinct()
            .CountAsync();

        var users = await _userManager.Users
            .Select(u => new { u.Id, u.UserName, u.Email })
            .ToListAsync();

        // get topic counts by author name
        var topicGroups = await _db.Topics
            .GroupBy(t => t.AuthorName)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToListAsync();

        var commentGroups = await _db.Comments
            .GroupBy(c => c.AuthorName)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToListAsync();

        // last login per key (userId or username)
        var lastLoginGroups = await _db.LoginActivities
            .GroupBy(a => a.UserId ?? a.Username)
            .Select(g => new { Key = g.Key, Last = g.Max(x => x.CreatedAtUtc) })
            .ToListAsync();

        // pull last activity descriptions for recent activities (in memory for matching keys)
        var recentActivities = await _db.LoginActivities
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(1000)
            .ToListAsync();

        var vm = new UserActivityIndexViewModel
        {
            ActiveUserCount = activeCount,
            TotalUsers = users.Count
        };

        foreach (var u in users)
        {
            var item = new UserActivityItem
            {
                UserId = u.Id,
                Username = u.UserName ?? string.Empty,
                Email = u.Email,
                TopicCount = topicGroups.FirstOrDefault(t => t.Name == u.UserName)?.Count ?? 0,
                CommentCount = commentGroups.FirstOrDefault(c => c.Name == u.UserName)?.Count ?? 0,
                LastLoginUtc = lastLoginGroups.FirstOrDefault(l => l.Key == u.Id || l.Key == u.UserName)?.Last,
                IsActive = lastLoginGroups.Any(l => (l.Key == u.Id || l.Key == u.UserName) && l.Last >= activeWindow)
            };

            var recent = recentActivities.FirstOrDefault(a => (a.UserId == u.Id) || a.Username == u.UserName);
            if (recent != null)
            {
                item.LastActivityDescription = recent.Description;
            }

            vm.Users.Add(item);
        }

        // sort by recent activity / active first
        vm.Users = vm.Users.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.LastLoginUtc ?? DateTime.MinValue).ToList();

        return View(vm);
    }
}
