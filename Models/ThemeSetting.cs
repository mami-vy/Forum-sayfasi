namespace mym.Models;

public class ThemeSetting
{
    public int Id { get; set; }
    public bool IsDarkMode { get; set; }
    public string PrimaryColor { get; set; } = "#2563eb";
    public string SecondaryColor { get; set; } = "#0f172a";
    public string AccentColor { get; set; } = "#f97316";
    public string BackgroundColor { get; set; } = "#f8fafc";
    public string SurfaceColor { get; set; } = "#ffffff";
    public string TextColor { get; set; } = "#0f172a";
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
