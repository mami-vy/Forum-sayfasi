using System.ComponentModel.DataAnnotations;

namespace mym.Models;

public class SiteSettingsViewModel
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string SiteName { get; set; } = "MyM";

    [StringLength(1000)]
    public string SiteDescription { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(256)]
    public string ContactEmail { get; set; } = string.Empty;

    [Url]
    [StringLength(1024)]
    public string LogoUrl { get; set; } = string.Empty;

    public bool AllowRegistration { get; set; } = true;
}
