using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mym.Models;

public class Comment
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Yorum içeriği gereklidir")]
    [Display(Name = "İçerik")]
    [StringLength(2000, MinimumLength = 2, ErrorMessage = "İçerik 2-2000 karakter arasında olmalıdır")]
    public string? Content { get; set; }

    [Display(Name = "Yazarın Adı")]
    public string? AuthorName { get; set; }

    [Display(Name = "Yazarın E-postası")]
    public string? AuthorEmail { get; set; }

    [Display(Name = "Konu")]
    [ForeignKey("Topic")]
    public int TopicId { get; set; }

    [Display(Name = "Oluşturma Tarihi")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [Display(Name = "Beğeni Sayısı")]
    public int LikeCount { get; set; } = 0;

    public virtual Topic? Topic { get; set; }
}
