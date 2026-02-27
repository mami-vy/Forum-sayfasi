using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using mym.Data;
using mym.Models;
using mym.Services;

namespace mym.Controllers;

[AutoValidateAntiforgeryToken]
public class AccountController : Controller
{
    private const string TempDataMessageKey = "Message";

    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IUserApprovalSettings _userApprovalSettings;
    private readonly ApplicationDbContext _dbContext;
    private readonly IAdminNotificationService _adminNotificationService;

    public AccountController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IUserApprovalSettings userApprovalSettings,
        ApplicationDbContext dbContext,
        IAdminNotificationService adminNotificationService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userApprovalSettings = userApprovalSettings;
        _dbContext = dbContext;
        _adminNotificationService = adminNotificationService;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Create(AccountCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new AppUser
        {
            UserName = model.Username,
            Email = model.Eposta,
            EmailConfirmed = _userApprovalSettings.AutoApproveNewUsers,
            IsActive = true,
            LegalTermsAcceptedAtUtc = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddClaimAsync(user, new Claim("legal_terms_accepted", "v1"));
            await _userManager.AddClaimAsync(user, new Claim("legal_terms_accepted_at_utc", DateTime.UtcNow.ToString("O")));

            TempData[TempDataMessageKey] = _userApprovalSettings.AutoApproveNewUsers
                ? "Kayit basarili. Hesabiniz otomatik onaylandi."
                : "Kayit basarili. Hesabiniz admin onayindan sonra aktif olacak.";
            return RedirectToAction(nameof(Login));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult CreateGuest()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> CreateGuestPost()
    {
        // create a temporary guest user, sign in, and set expiry to 2 days
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        var username = $"guest_{suffix}";
        var email = $"{username}@guest.local";

        var user = new AppUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true,
            IsActive = true,
            IsGuest = true,
            GuestExpiresAtUtc = DateTime.UtcNow.AddDays(2),
            LegalTermsAcceptedAtUtc = DateTime.UtcNow
        };

        // generate a random password so the account exists but is not easily claimed
        var pwd = "G" + Guid.NewGuid().ToString("N").Substring(0, 12) + "!";
        var result = await _userManager.CreateAsync(user, pwd);
        if (!result.Succeeded)
        {
            TempData[TempDataMessageKey] = "Misafir hesabı oluşturulamadı.";
            return RedirectToAction("CreateGuest");
        }

        // ensure role exists and add
        if (await _userManager.IsInRoleAsync(user, "Ziyaretci") == false)
        {
            await _userManager.AddToRoleAsync(user, "Ziyaretci");
        }

        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("guest", "true"));

        await _signInManager.SignInAsync(user, isPersistent: false);
        await AddLoginActivity(user.Id, user.UserName ?? username, true, "Misafir hesap olusturuldu ve giris yapildi.");

        TempData[TempDataMessageKey] = "Misafir hesap oluşturuldu - 2 gün sonra otomatik silinecek.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new AccountLoginModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(AccountLoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null)
        {
            await AddLoginActivity(null, model.Username, false, "Kullanici bulunamadi.");
            ModelState.AddModelError(string.Empty, "Kullanici adi veya sifre hatali.");
            return View(model);
        }

        if (!user.IsActive)
        {
            await AddLoginActivity(user.Id, user.UserName ?? model.Username, false, "Hesap pasif durumda.");
            ModelState.AddModelError(string.Empty, "Hesabiniz pasif durumda.");
            return View(model);
        }

        if (user.IsBlocked)
        {
            await AddLoginActivity(user.Id, user.UserName ?? model.Username, false, "Hesap engellendi.");
            ModelState.AddModelError(string.Empty, "Hesabiniz engellendi. Lutfen yonetici ile iletisime gecin.");
            return View(model);
        }

        var signInResult = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true
            // <-- bu  değer sonunda true olarak  değişcek-->
            );

        if (!signInResult.Succeeded)
        {
            await AddLoginActivity(user.Id, user.UserName ?? model.Username, false, "Hatali sifre denemesi.");
            await _adminNotificationService.CreateAsync(
                "Hatali giris denemesi",
                $"{user.UserName} kullanicisi icin hatali sifre denemesi oldu.",
                "warning");
            ModelState.AddModelError(string.Empty, "Kullanici adi veya sifre hatali.");
            return View(model);
        }

        if (!user.EmailConfirmed)
        {
            await _signInManager.SignOutAsync();
            await AddLoginActivity(user.Id, user.UserName ?? model.Username, false, "Hesap admin onayi bekliyor.");
            ModelState.AddModelError(string.Empty, "Hesabiniz henuz admin tarafindan onaylanmadi.");
            return View(model);
        }

        user.LastSeenAtUtc = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        await AddLoginActivity(user.Id, user.UserName ?? model.Username, true, "Basarili giris.");

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> MyAccount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var claims = await _userManager.GetClaimsAsync(user);
        var vm = new MyAccountViewModel
        {
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Bio = claims.FirstOrDefault(c => c.Type == UserProfileClaimTypes.Bio)?.Value ?? string.Empty,
            EducationTitle = claims.FirstOrDefault(c => c.Type == UserProfileClaimTypes.EducationTitle)?.Value
                ?? UserProfileClaimTypes.DefaultEducationTitle
        };

        return View(vm);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MyAccount(MyAccountViewModel model)
    {
        if (!UserProfileClaimTypes.EducationTitles.Contains(model.EducationTitle))
        {
            ModelState.AddModelError(nameof(model.EducationTitle), "Gecersiz unvan secimi.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var claims = await _userManager.GetClaimsAsync(user);
        await UpsertClaim(user, claims, UserProfileClaimTypes.Bio, model.Bio.Trim());
        await UpsertClaim(user, claims, UserProfileClaimTypes.EducationTitle, model.EducationTitle);

        TempData[TempDataMessageKey] = "Profil bilgilerin guncellendi.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> ProfileCard([FromQuery] string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest();
        }

        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            return NotFound();
        }

        var claims = await _userManager.GetClaimsAsync(user);
        var bio = claims.FirstOrDefault(c => c.Type == UserProfileClaimTypes.Bio)?.Value ?? "Bio girilmemis.";
        var educationTitle = claims.FirstOrDefault(c => c.Type == UserProfileClaimTypes.EducationTitle)?.Value
            ?? UserProfileClaimTypes.DefaultEducationTitle;
        TempData[TempDataMessageKey] = "Profil bilgilerin guncellendi.";

        return Json(new
        {
            username = user.UserName,
            educationTitle,
            bio
        });

    }

    private async Task UpsertClaim(AppUser user, IList<Claim> claims, string claimType, string value)
    {
        var existing = claims.FirstOrDefault(c => c.Type == claimType);
        if (existing == null)
        {
            await _userManager.AddClaimAsync(user, new Claim(claimType, value));
            return;
        }

        if (!string.Equals(existing.Value, value, StringComparison.Ordinal))
        {
            await _userManager.ReplaceClaimAsync(user, existing, new Claim(claimType, value));
        }
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task AddLoginActivity(string? userId, string username, bool isSuccess, string description)
    {
        _dbContext.LoginActivities.Add(new LoginActivity
        {
            UserId = userId,
            Username = username,
            IsSuccess = isSuccess,
            Description = description,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            CreatedAtUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
