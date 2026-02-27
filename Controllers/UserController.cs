using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mym.Data;
using mym.Models;
using mym.Services;

namespace mym.Controllers;

[Authorize(Policy = "ApproveUsers")]
public class UserController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IUserApprovalSettings _userApprovalSettings;
    private readonly ApplicationDbContext _dbContext;
    private readonly IAdminNotificationService _adminNotificationService;

    public UserController(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        SignInManager<AppUser> signInManager,
        IUserApprovalSettings userApprovalSettings,
        ApplicationDbContext dbContext,
        IAdminNotificationService adminNotificationService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _userApprovalSettings = userApprovalSettings;
        _dbContext = dbContext;
        _adminNotificationService = adminNotificationService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string search = "", string status = "all", string role = "all")
    {
        var users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();
        var rolesByUserId = new Dictionary<string, List<string>>();
        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            rolesByUserId[user.Id] = userRoles.OrderBy(x => x).ToList();
        }

        IEnumerable<AppUser> filtered = users;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowered = search.Trim().ToLowerInvariant();
            filtered = filtered.Where(u =>
                (u.UserName ?? string.Empty).ToLowerInvariant().Contains(lowered) ||
                (u.Email ?? string.Empty).ToLowerInvariant().Contains(lowered));
        }

        filtered = status.ToLowerInvariant() switch
        {
            "active" => filtered.Where(u => u.IsActive && !u.IsBlocked),
            "passive" => filtered.Where(u => !u.IsActive),
            "blocked" => filtered.Where(u => u.IsBlocked),
            "pending" => filtered.Where(u => !u.EmailConfirmed),
            _ => filtered
        };

        if (!string.Equals(role, "all", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(u =>
                rolesByUserId.TryGetValue(u.Id, out var rs) &&
                rs.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase)));
        }

        var vm = new UserIndexViewModel
        {
            Users = users,
            FilteredUsers = filtered.ToList(),
            AutoApproveNewUsers = _userApprovalSettings.AutoApproveNewUsers,
            AvailableRoles = _roleManager.Roles
                .Select(r => r.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .OrderBy(name => name)
                .ToList(),
            RolesByUserId = rolesByUserId,
            LoginActivities = await _dbContext.LoginActivities
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(50)
                .ToListAsync(),
            Search = search,
            StatusFilter = status,
            RoleFilter = role,
            TotalCount = users.Count,
            ActiveCount = users.Count(x => x.IsActive && !x.IsBlocked),
            BlockedCount = users.Count(x => x.IsBlocked),
            PendingCount = users.Count(x => !x.EmailConfirmed)
        };

        return View(vm);
    }

    [Authorize(Policy = "FullAccess")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetAutoApprove([FromForm] string[] enabled)
    {
        var isEnabled = enabled.Any(v => string.Equals(v, "true", StringComparison.OrdinalIgnoreCase));
        _userApprovalSettings.AutoApproveNewUsers = isEnabled;
        TempData["Message"] = isEnabled
            ? "Otomatik onay acildi. Yeni kayitlar dogrudan onayli olusturulacak."
            : "Otomatik onay kapatildi. Yeni kayitlar admin onayi bekleyecek.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "FullAccess")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetAutoApproveAdmin([FromForm] string[] enabled)
    {
        // kept for explicit admin-only control if needed (calls same logic)
        return SetAutoApprove(enabled);
    }

    [Authorize(Policy = "FullAccess")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(AdminUserCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Message"] = "Kullanici olusturulamadi. Alanlari kontrol edin.";
            return RedirectToAction(nameof(Index));
        }

        var user = new AppUser
        {
            UserName = model.Username,
            Email = model.Email,
            EmailConfirmed = model.EmailConfirmed,
            IsActive = model.IsActive,
            IsBlocked = false
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            TempData["Message"] = string.Join(" ", createResult.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        await _adminNotificationService.CreateAsync(
            "Yeni kullanici eklendi",
            $"{user.UserName} kullanicisi yonetim panelinden eklendi.",
            "success");

        TempData["Message"] = "Kullanici basariyla eklendi.";
        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(AdminUserEditModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Message"] = "Kullanici guncellenemedi. Alanlari kontrol edin.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null)
        {
            TempData["Message"] = "Kullanici bulunamadi.";
            return RedirectToAction(nameof(Index));
        }

        user.UserName = model.Username;
        user.Email = model.Email;
        user.IsActive = model.IsActive;
        user.IsBlocked = model.IsBlocked;
        user.EmailConfirmed = model.EmailConfirmed;
        user.LockoutEnd = model.IsBlocked ? DateTimeOffset.UtcNow.AddYears(100) : null;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            TempData["Message"] = string.Join(" ", updateResult.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        await _adminNotificationService.CreateAsync(
            "Kullanici duzenlendi",
            $"{user.UserName} kullanicisinin bilgileri guncellendi.",
            "info");

        TempData["Message"] = "Kullanici bilgileri guncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "FullAccess")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            TempData["Message"] = "Kullanici bulunamadi.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["Message"] = "Kullanici bulunamadi.";
            return RedirectToAction(nameof(Index));
        }

        var current = await _userManager.GetUserAsync(User);
        if (current != null && current.Id == user.Id)
        {
            TempData["Message"] = "Kendi hesabinizi silemezsiniz.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            TempData["Message"] = string.Join(" ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        await _adminNotificationService.CreateAsync(
            "Kullanici silindi",
            $"{user.UserName} kullanicisi sistemden silindi.",
            "danger");

        TempData["Message"] = "Kullanici silindi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["Message"] = "Kullanici bulunamadi.";
            return RedirectToAction(nameof(Index));
        }

        user.IsActive = !user.IsActive;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            TempData["Message"] = string.Join(" ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        await _adminNotificationService.CreateAsync(
            "Kullanici durumu guncellendi",
            $"{user.UserName} kullanicisi {(user.IsActive ? "aktif" : "pasif")} yapildi.",
            "warning");

        TempData["Message"] = $"{user.UserName} {(user.IsActive ? "aktif" : "pasif")} yapildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["Message"] = "Kullanici bulunamadi.";
            return RedirectToAction(nameof(Index));
        }

        user.IsBlocked = !user.IsBlocked;
        user.LockoutEnd = user.IsBlocked ? DateTimeOffset.UtcNow.AddYears(100) : null;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            TempData["Message"] = string.Join(" ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        await _adminNotificationService.CreateAsync(
            "Kullanici engel durumu degisti",
            $"{user.UserName} kullanicisi {(user.IsBlocked ? "engellendi" : "engel kaldirildi")}.",
            "warning");

        TempData["Message"] = user.IsBlocked
            ? $"{user.UserName} engellendi."
            : $"{user.UserName} engeli kaldirildi.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string id, string decision)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            TempData["Message"] = "Kullanici bulunamadi.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["Message"] = "Kullanici bulunamadi.";
            return RedirectToAction(nameof(Index));
        }

        if (string.Equals(decision, "approve", StringComparison.OrdinalIgnoreCase))
        {
            user.EmailConfirmed = true;
            user.LockoutEnd = null;
            user.IsBlocked = false;
            TempData["Message"] = $"{user.UserName} kullanicisi onaylandi.";
        }
        else if (string.Equals(decision, "reject", StringComparison.OrdinalIgnoreCase))
        {
            user.EmailConfirmed = false;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
            user.IsBlocked = true;
            TempData["Message"] = $"{user.UserName} kullanicisi reddedildi ve engellendi.";
        }
        else
        {
            TempData["Message"] = "Gecersiz islem.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            TempData["Message"] = string.Join(" ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "FullAccess")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRole(string id, string roleName)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            TempData["Message"] = "Kullanici bulunamadi.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(roleName))
        {
            TempData["Message"] = "Lutfen bir rol secin.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["Message"] = "Kullanici bulunamadi.";
            return RedirectToAction(nameof(Index));
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            TempData["Message"] = "Secilen rol sistemde bulunamadi.";
            return RedirectToAction(nameof(Index));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        if (currentRoles.Count == 1 && string.Equals(currentRoles[0], roleName, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Message"] = $"{user.UserName} zaten {roleName} rolune sahip.";
            return RedirectToAction(nameof(Index));
        }

        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                TempData["Message"] = string.Join(" ", removeResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }
        }

        var addResult = await _userManager.AddToRoleAsync(user, roleName);
        TempData["Message"] = addResult.Succeeded
            ? $"{user.UserName} kullanicisina {roleName} rolu verildi."
            : string.Join(" ", addResult.Errors.Select(e => e.Description));

        if (addResult.Succeeded)
        {
            await _adminNotificationService.CreateAsync(
                "Rol atamasi yapildi",
                $"{user.UserName} kullanicisina {roleName} rolu atandi.",
                "info");

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == user.Id)
            {
                await _signInManager.RefreshSignInAsync(user);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Notifications()
    {
        var items = await _adminNotificationService.GetLatestAsync(10);
        var unreadCount = await _adminNotificationService.GetUnreadCountAsync();

        return Json(new
        {
            unreadCount,
            items = items.Select(x => new
            {
                x.Id,
                x.Title,
                x.Message,
                x.Type,
                x.IsRead,
                createdAt = x.CreatedAtUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
            })
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkNotificationsRead()
    {
        await _adminNotificationService.MarkAllAsReadAsync();
        return Ok();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
