using System.ComponentModel.DataAnnotations;

namespace mym.Models;

public class AdminUserCreateModel
{
    [Required]
    [Display(Name = "Kullanici Adi")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [Display(Name = "Sifre")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Onayli")]
    public bool EmailConfirmed { get; set; } = true;
}
