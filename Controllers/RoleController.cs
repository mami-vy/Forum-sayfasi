using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using mym.Models;

namespace mym.Controllers;

[Authorize(Policy = "FullAccess")]
[AutoValidateAntiforgeryToken]
public class RoleController : Controller
{
    private const string TempDataMessageKey = "Message";

    private readonly RoleManager<AppRole> _roleManager;

    public RoleController(RoleManager<AppRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public IActionResult Index()
    {
        var roles = _roleManager.Roles
            .OrderBy(r => r.Name)
            .ToList();

        return View(roles);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleCreateModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _roleManager.CreateAsync(new AppRole { Name = model.Name });

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        return View(model);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var entity = await _roleManager.FindByIdAsync(id);

        if (entity != null)
        {
            return View(new RoleEditModel { Id = entity.Id, Name = entity.Name });
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, RoleEditModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _roleManager.FindByIdAsync(id);

            if (entity != null)
            {
                entity.Name = model.Name;
                var result = await _roleManager.UpdateAsync(entity);

                if (result.Succeeded)
                {
                    TempData[TempDataMessageKey] = "Rol basariyla guncellendi.";
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
        }

        return View(model);
    }

    public async Task<IActionResult> Delete(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToAction("Index");
        }

        var entity = await _roleManager.FindByIdAsync(id);
        if (entity == null)
        {
            return RedirectToAction("Index");
        }

        return View(entity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToAction("Index");
        }

        var entity = await _roleManager.FindByIdAsync(id);
        if (entity == null)
        {
            return RedirectToAction("Index");
        }

        var result = await _roleManager.DeleteAsync(entity);
        if (result.Succeeded)
        {
            TempData[TempDataMessageKey] = "Rol basariyla silindi.";
            return RedirectToAction("Index");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
        TempData["İnfoMessage"] = $"{entity.Name} Rolü Başarılı Bir Şekilde Silindi";
        return View(entity);
    }
}
