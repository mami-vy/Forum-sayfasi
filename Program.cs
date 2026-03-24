using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using mym.Data;
using System.Security.Claims;
using System.Threading.RateLimiting;
using mym.Models;
using mym.Services;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSingleton<IUserApprovalSettings, UserApprovalSettings>();
builder.Services.AddScoped<IThemeSettingsService, ThemeSettingsService>();
builder.Services.AddScoped<IAdminNotificationService, AdminNotificationService>();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddHostedService<GuestCleanupService>();

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddRazorPages();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});

builder.Services.Configure<IdentityOptions>(Options =>
{
    Options.Password.RequireDigit = false;
    Options.Password.RequiredLength = 6;
    Options.Password.RequireNonAlphanumeric = false;
    Options.Password.RequireUppercase = false;
    Options.Password.RequireLowercase = false;
    Options.User.RequireUniqueEmail = true;
    Options.Lockout.MaxFailedAccessAttempts = 3;
    Options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
});

builder.Services.ConfigureApplicationCookie(Options =>
{
    Options.LoginPath = "/Account/Login";
    Options.LogoutPath = "/Account/Logout";
    Options.AccessDeniedPath = "/Account/AccessDenied";
    Options.ExpireTimeSpan = TimeSpan.FromDays(28);
    Options.SlidingExpiration = true;
    Options.Cookie.HttpOnly = true;
    Options.Cookie.SameSite = SameSiteMode.Lax;
    Options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Authorization policies (role names are normalized to handle variants)
builder.Services.AddAuthorization(options =>
{
    static string NormalizeRoleKey(string? roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName)) return string.Empty;
        return roleName.Trim().ToLowerInvariant()
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace("Ä±", "i")
            .Replace("ÄŸ", "g")
            .Replace("Ã¼", "u")
            .Replace("ÅŸ", "s")
            .Replace("Ã¶", "o")
            .Replace("Ã§", "c");
    }

    // Full admin (role assignment etc.)
    options.AddPolicy("FullAccess", policy => policy.RequireAssertion(context =>
    {
        var roles = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => NormalizeRoleKey(c.Value));
        return roles.Any(r => r == "administrator" || r == "admin");
    }));

    // Role management (admin + super-moderator)
    options.AddPolicy("ManageRoles", policy => policy.RequireAssertion(context =>
    {
        var roles = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => NormalizeRoleKey(c.Value));
        return roles.Any(r => r == "administrator" || r == "admin" || r.Contains("super"));
    }));

    // Manage site settings (admin + super-moderator)
    options.AddPolicy("ManageSiteSettings", policy => policy.RequireAssertion(context =>
    {
        var roles = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => NormalizeRoleKey(c.Value));
        return roles.Any(r => r == "administrator" || r.Contains("super"));
    }));

    // View activity (admin + super-moderator)
    options.AddPolicy("ViewActivity", policy => policy.RequireAssertion(context =>
    {
        var roles = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => NormalizeRoleKey(c.Value));
        return roles.Any(r => r == "administrator" || r.Contains("super"));
    }));

    // Approve users / basic moderation (any role containing 'moderat' or 'caylak' or admin)
    options.AddPolicy("ApproveUsers", policy => policy.RequireAssertion(context =>
    {
        var roles = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => NormalizeRoleKey(c.Value));
        return roles.Any(r => r == "administrator" || r.Contains("moderat") || r.Contains("caylak"));
    }));
});

var app = builder.Build();



// Create database and apply migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Apply migrations at startup (development convenience)
        context.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database initialization error: {ex.Message}");
    }

    // Ensure core roles exist
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        var roleNames = new[] { "Administrator", "Moderator", "SuperModerator", "CaylakModerator", "Uye", "Ziyaretci" };
        foreach (var rn in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(rn))
            {
                await roleManager.CreateAsync(new AppRole { Name = rn });
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Role creation error: {ex.Message}");
    }

    // Ensure display-friendly variants exist: rename duplicates if present
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        // normalize a few common variants by merging into canonical names
        var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "SuperModeratör", "SuperModerator" },
            { "Süper moderatör", "SuperModerator" },
            { "Çaylak Moderatör", "CaylakModerator" },
            { "ÇaylakModerator", "CaylakModerator" },
            { "moderatör", "Moderator" },
            { "Üye", "Uye" },
            { "SuperModeratÃ¶r", "SuperModerator" },
            { "SÃ¼per moderatÃ¶r", "SuperModerator" },
            { "Ã‡aylak ModeratÃ¶r", "CaylakModerator" },
            { "Ã‡aylakModerator", "CaylakModerator" },
            { "moderatÃ¶r", "Moderator" },
            { "administrator", "Administrator" },
            { "Admin", "Administrator" }
        };

        foreach (var kv in mappings)
        {
            var old = kv.Key;
            var target = kv.Value;
            var existing = await roleManager.FindByNameAsync(old);
            if (existing != null)
            {
                // if target exists, move users; otherwise rename
                var tgt = await roleManager.FindByNameAsync(target);
                if (tgt == null)
                {
                    existing.Name = target;
                    await roleManager.UpdateAsync(existing);
                }
                else
                {
                    // move users from existing to target
                    // we cannot access UserManager here without complexity; skip moving automatically
                }
            }
        }
    }
    catch { }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "img-src 'self' data: https:; " +
        "style-src 'self' 'unsafe-inline' https://cdn.tailwindcss.com https://cdnjs.cloudflare.com; " +
        "script-src 'self' 'unsafe-inline' https://cdn.tailwindcss.com; " +
        "font-src 'self' https://cdnjs.cloudflare.com data:; " +
        "frame-ancestors 'none'; base-uri 'self'; form-action 'self';";
    await next();
});
app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();




