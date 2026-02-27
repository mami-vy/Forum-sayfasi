using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mym.Data;
using mym.Models;

namespace mym.Controllers;

[AutoValidateAntiforgeryToken]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var forums = await _context.Forums
            .Include(f => f.Topics!.Where(t => t.IsPublished))
            .ThenInclude(t => t.Comments)
            .OrderBy(f => f.Id)
            .ToListAsync();

        foreach (var forum in forums)
        {
            forum.TopicCount = forum.Topics?.Count ?? 0;
            forum.CommentCount = forum.Topics?.Sum(t => t.Comments?.Count ?? 0) ?? 0;
        }

        var recentTopics = await _context.Topics
            .Where(t => t.IsPublished)
            .Include(t => t.Forum)
            .Include(t => t.Comments)
            .OrderByDescending(t => t.CreatedDate)
            .Take(3)
            .ToListAsync();

        foreach (var topic in recentTopics)
        {
            topic.CommentCount = topic.Comments?.Count ?? topic.CommentCount;
        }

        var totalTopics = await _context.Topics.CountAsync(t => t.IsPublished);
        var totalComments = await _context.Comments.CountAsync(c => c.Topic.IsPublished);
        var totalUsers = await _context.Users.CountAsync();

        ViewBag.Forums = forums;
        ViewBag.RecentTopics = recentTopics;
        ViewBag.TotalTopics = totalTopics;
        ViewBag.TotalComments = totalComments;
        ViewBag.TotalUsers = totalUsers;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Terms()
    {
        return View();
    }

    [HttpGet]
    public IActionResult CookiePolicy()
    {
        return View();
    }

    [HttpGet("/aydinlatma-metni")]
    public IActionResult AydinlatmaMetni()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
