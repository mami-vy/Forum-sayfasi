namespace mym.Models.Admin;

public class UserActivityItem
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int TopicCount { get; set; }
    public int CommentCount { get; set; }
    public DateTime? LastLoginUtc { get; set; }
    public string? LastActivityDescription { get; set; }
    public bool IsActive { get; set; }
}
