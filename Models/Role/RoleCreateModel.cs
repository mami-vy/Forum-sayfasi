using System.ComponentModel.DataAnnotations;

namespace mym.Models;

public class RoleCreateModel
{
    [Required]
    [Display(Name = "Rol Adı")]
    public string? Name { get; set; }
    [Display(Name = "E-posta Adresi")]
    public string? Email { get; set; }
    [StringLength(50)]
    public string? DisplayName { get; set; } // Forum'da görünecek isim

    [StringLength(500)]
    public string Bio { get; set; } = "";

    [StringLength(200)]
    public string? ProfileImageUrl { get; set; }

}