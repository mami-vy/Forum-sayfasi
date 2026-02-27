using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mym.Data;
using mym.Models;

namespace mym.Controllers;

[AutoValidateAntiforgeryToken]
public class TopicController : Controller
{
    private const string ForumControllerName = "Forum";
    private const string DetailsActionName = "Details";
    private const string TempDataMessageKey = "Message";

    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public TopicController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Topic/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id == null)
        {
            return NotFound();
        }

        var topic = await _context.Topics
            .Include(t => t.Forum)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (topic == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var canModerate = await IsModeratorAsync(currentUser);
        if (!topic.IsPublished && !canModerate)
        {
            return NotFound();
        }

        topic.ViewCount++;
        await _context.SaveChangesAsync();

        ViewBag.CanPostComment = currentUser?.EmailConfirmed == true && topic.IsPublished;
        ViewBag.CanModerate = canModerate;

        return View(topic);
    }

    // GET: Topic/Create
    [Authorize]
    public async Task<IActionResult> Create(int? forumId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (forumId == null)
        {
            return BadRequest();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.EmailConfirmed != true)
        {
            TempData[TempDataMessageKey] = "Konu acmak icin hesabin admin tarafindan onaylanmis olmali.";
            return RedirectToAction(DetailsActionName, ForumControllerName, new { id = forumId });
        }

        ViewBag.ForumId = forumId;
        return View();
    }

    // POST: Topic/Create
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int forumId, [Bind("Title,Content")] Topic topic)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ForumId = forumId;
            return View(topic);
        }

        var forum = await _context.Forums.FindAsync(forumId);
        if (forum == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.EmailConfirmed != true)
        {
            TempData[TempDataMessageKey] = "Konu acmak icin hesabin admin tarafindan onaylanmis olmali.";
            return RedirectToAction(DetailsActionName, ForumControllerName, new { id = forumId });
        }

        topic.ForumId = forumId;
        topic.AuthorName = currentUser.UserName;
        var educationTitle = await ResolveEducationTitle(currentUser);
        topic.AuthorEmail = educationTitle;
        topic.CreatedDate = DateTime.Now;
        topic.IsPublished = true;

        _context.Add(topic);
        await _context.SaveChangesAsync();

        forum.TopicCount = await _context.Topics.CountAsync(t => t.ForumId == forumId && t.IsPublished);
        forum.CommentCount = await (from c in _context.Comments
                                    join t in _context.Topics on c.TopicId equals t.Id
                                    where t.ForumId == forumId && t.IsPublished
                                    select c).CountAsync();
        await _context.SaveChangesAsync();

        return RedirectToAction(DetailsActionName, ForumControllerName, new { id = forumId });
    }

    // GET: Topic/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id == null)
        {
            return NotFound();
        }

        var topic = await _context.Topics.FindAsync(id);
        if (topic == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var isModerator = await IsModeratorAsync(currentUser);
        if (!isModerator && (currentUser == null || currentUser.UserName != topic.AuthorName))
        {
            return Forbid();
        }

        return View(topic);
    }

    // POST: Topic/Edit/5
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,ForumId")] Topic topic)
    {
        if (!ModelState.IsValid)
        {
            return View(topic);
        }

        if (id != topic.Id)
        {
            return NotFound();
        }

        var existing = await _context.Topics.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (existing == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var isModerator = await IsModeratorAsync(currentUser);
        if (!isModerator && (currentUser == null || currentUser.UserName != existing.AuthorName))
        {
            return Forbid();
        }

        try
        {
            topic.AuthorName = existing.AuthorName;
            topic.AuthorEmail = existing.AuthorEmail;
            topic.CreatedDate = existing.CreatedDate;
            topic.ViewCount = existing.ViewCount;
            topic.CommentCount = existing.CommentCount;
            topic.UpdatedDate = DateTime.Now;
            topic.IsPublished = existing.IsPublished;

            _context.Update(topic);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TopicExists(topic.Id))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction("Details", new { id = topic.Id });
    }

    // GET: Topic/Delete/5
    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id == null)
        {
            return NotFound();
        }

        var topic = await _context.Topics
            .Include(t => t.Forum)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (topic == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var isModerator = await IsModeratorAsync(currentUser);
        if (!isModerator && (currentUser == null || currentUser.UserName != topic.AuthorName))
        {
            return Forbid();
        }

        return View(topic);
    }

    // POST: Topic/Delete/5
    [Authorize]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var topic = await _context.Topics.FindAsync(id);
        if (topic == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var isModerator = await IsModeratorAsync(currentUser);
        if (!isModerator && (currentUser == null || currentUser.UserName != topic.AuthorName))
        {
            return Forbid();
        }

        var forumId = topic.ForumId;
        _context.Topics.Remove(topic);
        await _context.SaveChangesAsync();

        var forum = await _context.Forums.FindAsync(forumId);
        if (forum != null)
        {
            forum.TopicCount = await _context.Topics.CountAsync(t => t.ForumId == forumId && t.IsPublished);
            forum.CommentCount = await (from c in _context.Comments
                                        join t in _context.Topics on c.TopicId equals t.Id
                                        where t.ForumId == forumId && t.IsPublished
                                        select c).CountAsync();
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(DetailsActionName, ForumControllerName, new { id = forumId });
    }

    private bool TopicExists(int id)
    {
        return _context.Topics.Any(e => e.Id == id);
    }

    private async Task<string> ResolveEducationTitle(AppUser user)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        var educationTitle = claims.FirstOrDefault(c => c.Type == UserProfileClaimTypes.EducationTitle)?.Value;
        return string.IsNullOrWhiteSpace(educationTitle)
            ? UserProfileClaimTypes.DefaultEducationTitle
            : educationTitle;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Kaldir(int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (!await IsModeratorAsync(currentUser))
        {
            return Forbid();
        }

        var topic = await _context.Topics.FindAsync(id);
        if (topic == null)
        {
            return NotFound();
        }

        topic.IsPublished = false;

        var forum = await _context.Forums.FindAsync(topic.ForumId);
        if (forum != null)
        {
            forum.TopicCount = await _context.Topics.CountAsync(t => t.ForumId == forum.Id && t.IsPublished);
            forum.CommentCount = await (from c in _context.Comments
                                        join t in _context.Topics on c.TopicId equals t.Id
                                        where t.ForumId == forum.Id && t.IsPublished
                                        select c).CountAsync();
        }

        await _context.SaveChangesAsync();

        TempData[TempDataMessageKey] = "Icerik yayindan kaldirildi.";
        return RedirectToAction(DetailsActionName, new { id });
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
