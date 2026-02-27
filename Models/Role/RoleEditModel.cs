using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace mym.Models;



public class RoleEditModel
{
    public string? Id { get; set; }  // int -> string

    [Required]
    [Display(Name = "Rol Adı")]
    public string? Name { get; set; }

    [Display(Name = "E-posta Adresi")]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? DisplayName { get; set; }
}