using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mym.Models;
using mym.Services;

namespace mym.Controllers;

[Authorize(Policy = "ManageSiteSettings")]
public class ThemeController : Controller
{
    private readonly IThemeSettingsService _themeSettingsService;
    private readonly IAdminNotificationService _adminNotificationService;

    public ThemeController(
        IThemeSettingsService themeSettingsService,
        IAdminNotificationService adminNotificationService)
    {
        _themeSettingsService = themeSettingsService;
        _adminNotificationService = adminNotificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var current = await _themeSettingsService.GetCurrentAsync();
        var vm = new ThemeSettingsViewModel
        {
            IsDarkMode = current.IsDarkMode,
            PrimaryColor = current.PrimaryColor,
            SecondaryColor = current.SecondaryColor,
            AccentColor = current.AccentColor,
            BackgroundColor = current.BackgroundColor,
            SurfaceColor = current.SurfaceColor,
            TextColor = current.TextColor
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ThemeSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Message"] = "Tema ayarlari gecersiz.";
            return View(model);
        }

        await _themeSettingsService.SaveAsync(model);
        await _adminNotificationService.CreateAsync(
            "Tema guncellendi",
            "Yonetim panelinden global tema ayarlari degistirildi.",
            "success");

        TempData["Message"] = "Tema ayarlari guncellendi.";
        return RedirectToAction(nameof(Index));
    }
}
