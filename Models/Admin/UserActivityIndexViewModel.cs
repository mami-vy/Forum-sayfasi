using System.Collections.Generic;

namespace mym.Models.Admin;

public class UserActivityIndexViewModel
{
    public int ActiveUserCount { get; set; }
    public int TotalUsers { get; set; }
    public List<UserActivityItem> Users { get; set; } = new();
}
