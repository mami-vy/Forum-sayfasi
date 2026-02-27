using Microsoft.EntityFrameworkCore;
using mym.Data;
using mym.Models;

namespace mym.Services;

public class ThemeSettingsService : IThemeSettingsService
{
    private readonly ApplicationDbContext _dbContext;

    public ThemeSettingsService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ThemeSetting> GetCurrentAsync()
    {
        var setting = await _dbContext.ThemeSettings.FirstOrDefaultAsync();
        if (setting != null)
        {
            return setting;
        }

        setting = new ThemeSetting();
        _dbContext.ThemeSettings.Add(setting);
        await _dbContext.SaveChangesAsync();
        return setting;
    }

    public async Task SaveAsync(ThemeSettingsViewModel model)
    {
        var setting = await GetCurrentAsync();
        setting.IsDarkMode = model.IsDarkMode;
        setting.PrimaryColor = model.PrimaryColor;
        setting.SecondaryColor = model.SecondaryColor;
        setting.AccentColor = model.AccentColor;
        setting.BackgroundColor = model.BackgroundColor;
        setting.SurfaceColor = model.SurfaceColor;
        setting.TextColor = model.TextColor;
        setting.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }
}
