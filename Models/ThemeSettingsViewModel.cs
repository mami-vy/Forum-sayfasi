using System.ComponentModel.DataAnnotations;

namespace mym.Models;

public class ThemeSettingsViewModel
{
    public bool IsDarkMode { get; set; }

    [Required]
    [RegularExpression("^#[0-9A-Fa-f]{6}$")]
    public string PrimaryColor { get; set; } = "#2563eb";

    [Required]
    [RegularExpression("^#[0-9A-Fa-f]{6}$")]
    public string SecondaryColor { get; set; } = "#0f172a";

    [Required]
    [RegularExpression("^#[0-9A-Fa-f]{6}$")]
    public string AccentColor { get; set; } = "#f97316";

    [Required]
    [RegularExpression("^#[0-9A-Fa-f]{6}$")]
    public string BackgroundColor { get; set; } = "#f8fafc";

    [Required]
    [RegularExpression("^#[0-9A-Fa-f]{6}$")]
    public string SurfaceColor { get; set; } = "#ffffff";

    [Required]
    [RegularExpression("^#[0-9A-Fa-f]{6}$")]
    public string TextColor { get; set; } = "#0f172a";
}
