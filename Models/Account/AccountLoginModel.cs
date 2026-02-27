using System.ComponentModel.DataAnnotations;

namespace mym.Models;

public class AccountLoginModel
{
    [Display(Name = "Kullanici Adi")]
    [Required]
    public string Username { get; set; } = null!;

    [Display(Name = "Sifre")]
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Display(Name = "Beni Hatirla")]
    public bool RememberMe { get; set; }
}
