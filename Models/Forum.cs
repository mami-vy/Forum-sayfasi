using System.ComponentModel.DataAnnotations;

namespace mym.Models;

public class Forum
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Forum başlığı gereklidir")]
    [Display(Name = "Forum Başlığı")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Forum başlığı 5-200 karakter arasında olmalıdır")]
    public string? Title { get; set; }

    [Display(Name = "Açıklama")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Kategori")]
    public string? Category { get; set; }

    [Display(Name = "İkon")]
    public string? Icon { get; set; }

    [Display(Name = "Oluşturma Tarihi")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [Display(Name = "Konu Sayısı")]
    public int TopicCount { get; set; } = 0;

    [Display(Name = "Yorum Sayısı")]
    public int CommentCount { get; set; } = 0;

    public virtual ICollection<Topic>? Topics { get; set; }
}
