using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mym.Data;
using mym.Models;

namespace mym.Controllers;

[AutoValidateAntiforgeryToken]
public class ForumController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public ForumController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Forum
    public async Task<IActionResult> Index()
    {
        var forums = await _context.Forums
            .Include(f => f.Topics!.Where(t => t.IsPublished))
            .ThenInclude(t => t.Comments)
            .OrderBy(f => f.Category)
            .ToListAsync();

        foreach (var forum in forums)
        {
            forum.TopicCount = forum.Topics?.Count ?? 0;
            forum.CommentCount = forum.Topics?.Sum(t => t.Comments?.Count ?? 0) ?? 0;
        }

        return View(forums);
    }

    // GET: Forum/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var forum = await _context.Forums
            .Include(f => f.Topics!.Where(t => t.IsPublished))
            .ThenInclude(t => t.Comments)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (forum == null)
        {
            return NotFound();
        }

        forum.TopicCount = forum.Topics?.Count ?? 0;
        forum.CommentCount = forum.Topics?.Sum(t => t.Comments?.Count ?? 0) ?? 0;

        var currentUser = await _userManager.GetUserAsync(User);
        ViewBag.CanCreateTopic = currentUser?.EmailConfirmed == true;

        return View(forum);
    }

    [Authorize(Policy = "FullAccess")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var forum = await _context.Forums
            .FirstOrDefaultAsync(m => m.Id == id);
        if (forum == null)
        {
            return NotFound();
        }

        return View(forum);
    }

    [Authorize(Policy = "FullAccess")]
    [ValidateAntiForgeryToken]
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var forum = await _context.Forums.FindAsync(id);
        if (forum != null)
        {
            _context.Forums.Remove(forum);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
