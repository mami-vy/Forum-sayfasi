using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mym.Data;
using mym.Models;

namespace mym.Controllers;

[AutoValidateAntiforgeryToken]
public class CommentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public CommentController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // POST: Comment/Create
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int topicId, [Bind("Content")] Comment comment)
    {
        var topic = await _context.Topics.FindAsync(topicId);
        if (topic == null || !topic.IsPublished)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.EmailConfirmed != true)
        {
            TempData["Message"] = "Yorum yazmak icin hesabinizin admin tarafindan onaylanmasi gerekiyor.";
            return RedirectToAction("Details", "Topic", new { id = topicId });
        }

        comment.TopicId = topicId;
        comment.AuthorName = currentUser.UserName;
        var educationTitle = await ResolveEducationTitle(currentUser);
        comment.AuthorEmail = educationTitle;
        comment.CreatedDate = DateTime.Now;

        if (!ModelState.IsValid)
        {
            return RedirectToAction("Details", "Topic", new { id = topicId });
        }

        _context.Add(comment);
        await _context.SaveChangesAsync();
        await RecalculateCountsAsync(topicId);

        return RedirectToAction("Details", "Topic", new { id = topicId });
    }

    // POST: Comment/Delete/5
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var comment = await _context.Comments
            .Include(c => c.Topic)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var isModerator = await IsModeratorAsync(currentUser);
        if (!isModerator && (currentUser == null || currentUser.UserName != comment.AuthorName))
        {
            return Forbid();
        }

        var topicId = comment.TopicId;

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        await RecalculateCountsAsync(topicId);

        return RedirectToAction("Details", "Topic", new { id = topicId });
    }

    // POST: Comment/Like/5
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Like(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
        {
            return NotFound();
        }

        comment.LikeCount++;
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Topic", new { id = comment.TopicId });
    }

    private async Task<string> ResolveEducationTitle(AppUser user)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        var educationTitle = claims.FirstOrDefault(c => c.Type == UserProfileClaimTypes.EducationTitle)?.Value;
        return string.IsNullOrWhiteSpace(educationTitle)
            ? UserProfileClaimTypes.DefaultEducationTitle
            : educationTitle;
    }

    private async Task RecalculateCountsAsync(int topicId)
    {
        var topic = await _context.Topics.FindAsync(topicId);
        if (topic == null)
        {
            return;
        }

        topic.CommentCount = await _context.Comments.CountAsync(c => c.TopicId == topicId);

        var forum = await _context.Forums.FindAsync(topic.ForumId);
        if (forum != null)
        {
            forum.TopicCount = await _context.Topics.CountAsync(t => t.ForumId == forum.Id && t.IsPublished);
            forum.CommentCount = await _context.Comments.CountAsync(c => c.Topic.ForumId == forum.Id && c.Topic.IsPublished);
        }

        await _context.SaveChangesAsync();
    }

    private async Task<bool> IsModeratorAsync(AppUser? user)
    {
        if (user == null)
        {
            return false;
        }

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            var key = NormalizeRoleKey(role);
            if (key == "administrator" || key == "admin" || key.Contains("moderat"))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeRoleKey(string? roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return string.Empty;
        }

        return roleName
            .Trim()
            .ToLowerInvariant()
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c");
    }
}
