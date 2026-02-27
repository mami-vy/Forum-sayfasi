using System.ComponentModel.DataAnnotations;

namespace mym.Models;

public class AccountCreateModel
{
    [Required]
    [Display(Name = "Adiniz")]
    public string Name { get; set; } = null!;

    [Required]
    [Display(Name = "Soyadiniz")]
    public string Surname { get; set; } = null!;

    [Required]
    [EmailAddress]
    [Display(Name = "E-posta Adresiniz")]
    public string Eposta { get; set; } = null!;

    [Required]
    [Display(Name = "Kullanici Adiniz")]
    public string Username { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Sifreniz")]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    [DataType(DataType.Password)]
    [Display(Name = "Sifreniz Tekrar")]
    [Compare("Password", ErrorMessage = "Sifreler uyusmuyor.")]
    public string? ConfirmPassword { get; set; }

    [Display(Name = "Aydinlatma metni ve kullanim kosullarini kabul ediyorum")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "Devam etmek icin yasal metinleri kabul etmelisiniz.")]
    public bool AcceptLegalTerms { get; set; }
}
