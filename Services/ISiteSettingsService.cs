using mym.Models;

namespace mym.Services;

public interface ISiteSettingsService
{
    Task<SiteSetting> GetCurrentAsync();
    Task SaveAsync(SiteSettingsViewModel model);
}
