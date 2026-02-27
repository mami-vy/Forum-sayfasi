using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace mym.Models;

public class MyAccountViewModel
{
    [Display(Name = "Kullanici Adi")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Bio")]
    [StringLength(300, ErrorMessage = "Bio en fazla 300 karakter olabilir.")]
    public string Bio { get; set; } = string.Empty;

    [Display(Name = "Egitim Unvani")]
    [Required(ErrorMessage = "Lutfen bir unvan sec.")]
    public string EducationTitle { get; set; } = UserProfileClaimTypes.DefaultEducationTitle;

    public List<SelectListItem> EducationOptions { get; set; } = UserProfileClaimTypes
        .EducationTitles
        .Select(title => new SelectListItem
        {
            Text = title,
            Value = title
        })
        .ToList();
}
