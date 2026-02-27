using Microsoft.EntityFrameworkCore;
using mym.Data;
using mym.Models;

namespace mym.Services;

public class SiteSettingsService : ISiteSettingsService
{
    private readonly ApplicationDbContext _dbContext;

    public SiteSettingsService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SiteSetting> GetCurrentAsync()
    {
        var setting = await _dbContext.SiteSettings.FirstOrDefaultAsync();
        if (setting != null) return setting;

        setting = new SiteSetting();
        _dbContext.SiteSettings.Add(setting);
        await _dbContext.SaveChangesAsync();
        return setting;
    }

    public async Task SaveAsync(SiteSettingsViewModel model)
    {
        var setting = await GetCurrentAsync();
        setting.SiteName = model.SiteName;
        setting.SiteDescription = model.SiteDescription;
        setting.ContactEmail = model.ContactEmail;
        setting.LogoUrl = model.LogoUrl;
        setting.AllowRegistration = model.AllowRegistration;
        setting.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }
}
