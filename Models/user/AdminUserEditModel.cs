using System.ComponentModel.DataAnnotations;

namespace mym.Models;

public class AdminUserEditModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Kullanici Adi")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; }

    [Display(Name = "Engelli")]
    public bool IsBlocked { get; set; }

    [Display(Name = "Onayli")]
    public bool EmailConfirmed { get; set; }
}
