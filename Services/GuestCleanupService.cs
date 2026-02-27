using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using mym.Models;

namespace mym.Services;

public class GuestCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<GuestCleanupService> _logger;

    public GuestCleanupService(IServiceProvider services, ILogger<GuestCleanupService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run cleanup every hour
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                var now = DateTime.UtcNow;
                var expired = userManager.Users
                    .Where(u => u.IsGuest && u.GuestExpiresAtUtc.HasValue && u.GuestExpiresAtUtc.Value <= now)
                    .ToList();

                foreach (var u in expired)
                {
                    try
                    {
                        _logger.LogInformation("Anonymizing expired guest user {UserId}", u.Id);

                        // remove roles
                        try
                        {
                            var roles = await userManager.GetRolesAsync(u);
                            if (roles?.Count > 0)
                            {
                                await userManager.RemoveFromRolesAsync(u, roles);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to remove roles for guest {UserId}", u.Id);
                        }

                        // remove claims
                        try
                        {
                            var claims = await userManager.GetClaimsAsync(u);
                            if (claims?.Count > 0)
                            {
                                await userManager.RemoveClaimsAsync(u, claims);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to remove claims for guest {UserId}", u.Id);
                        }

                        // anonymize and disable account
                        u.UserName = "deleted_guest_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                        u.NormalizedUserName = u.UserName.ToUpperInvariant();
                        u.Email = string.Empty;
                        u.NormalizedEmail = string.Empty;
                        u.EmailConfirmed = false;
                        u.IsActive = false;
                        u.IsBlocked = true;
                        u.IsGuest = false;
                        u.GuestExpiresAtUtc = null;
                        u.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);

                        await userManager.UpdateAsync(u);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error anonymizing guest user {UserId}", u.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Guest cleanup failed");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
