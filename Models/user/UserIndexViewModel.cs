using Microsoft.AspNetCore.Identity;

namespace mym.Models;

public class UserIndexViewModel
{
    public List<AppUser> Users { get; set; } = new();
    public List<AppUser> FilteredUsers { get; set; } = new();
    public bool AutoApproveNewUsers { get; set; }
    public List<string> AvailableRoles { get; set; } = new();
    public Dictionary<string, List<string>> RolesByUserId { get; set; } = new();
    public List<LoginActivity> LoginActivities { get; set; } = new();
    public string Search { get; set; } = string.Empty;
    public string StatusFilter { get; set; } = "all";
    public string RoleFilter { get; set; } = "all";
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int BlockedCount { get; set; }
    public int PendingCount { get; set; }
}
