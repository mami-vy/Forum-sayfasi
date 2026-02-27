namespace mym.Models;

public class SiteSetting
{
    public int Id { get; set; }
    public string SiteName { get; set; } = "MyM";
    public string SiteDescription { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public bool AllowRegistration { get; set; } = true;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
