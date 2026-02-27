using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mym.Models;
using mym.Services;

namespace mym.Controllers;

[Authorize(Policy = "ManageSiteSettings")]
public class SiteSettingsController : Controller
{
    private readonly ISiteSettingsService _siteSettingsService;
    private readonly IAdminNotificationService _adminNotificationService;

    public SiteSettingsController(ISiteSettingsService siteSettingsService, IAdminNotificationService adminNotificationService)
    {
        _siteSettingsService = siteSettingsService;
        _adminNotificationService = adminNotificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var current = await _siteSettingsService.GetCurrentAsync();
        var vm = new SiteSettingsViewModel
        {
            SiteName = current.SiteName,
            SiteDescription = current.SiteDescription,
            ContactEmail = current.ContactEmail,
            LogoUrl = current.LogoUrl,
            AllowRegistration = current.AllowRegistration
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SiteSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Message"] = "Site ayarlari gecersiz.";
            return View(model);
        }

        await _siteSettingsService.SaveAsync(model);
        await _adminNotificationService.CreateAsync("Site ayarlari guncellendi", "Yonetim panelinden site ayarlari guncellendi.", "success");

        TempData["Message"] = "Site ayarlari kaydedildi.";
        return RedirectToAction(nameof(Index));
    }
}
