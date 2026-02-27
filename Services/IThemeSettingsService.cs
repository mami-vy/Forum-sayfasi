using mym.Models;

namespace mym.Services;

public interface IThemeSettingsService
{
    Task<ThemeSetting> GetCurrentAsync();
    Task SaveAsync(ThemeSettingsViewModel model);
}
