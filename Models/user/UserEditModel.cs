using System.ComponentModel.DataAnnotations;

namespace mym.Models;

public class UserEditModel
{
    [Display(Name = "Adınız")]
    public string Name { get; set; } = null!;
    [Display(Name = "Soyadınız")]
    public string Surname { get; set; } = null!;
    [Display(Name = "E-posta Adresiniz")]
    public string Eposta { get; set; } = null!;
    [Display(Name = "Kullanıcı Adınız")]
    public string Username { get; set; } = null!;
    [Display(Name = "Şifreniz")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
    [Display(Name = "Şifreniz Tekrar")]
    [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }
    [Display(Name = "Profil Resmi")]

    public string? İmgUrl { get; set; }
}