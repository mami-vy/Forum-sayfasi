using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mym.Models;

public class Topic
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Konu başlığı gereklidir")]
    [Display(Name = "Konu Başlığı")]
    [StringLength(300, MinimumLength = 5, ErrorMessage = "Konu başlığı 5-300 karakter arasında olmalıdır")]
    public string? Title { get; set; }

    [Required(ErrorMessage = "Konu içeriği gereklidir")]
    [Display(Name = "İçerik")]
    [StringLength(5000, MinimumLength = 10, ErrorMessage = "İçerik 10-5000 karakter arasında olmalıdır")]
    public string? Content { get; set; }

    [Display(Name = "Yazarın Adı")]
    public string? AuthorName { get; set; }

    [Display(Name = "Yazarın E-postası")]
    public string? AuthorEmail { get; set; }

    [Display(Name = "Forum")]
    [ForeignKey("Forum")]
    public int ForumId { get; set; }

    [Display(Name = "Oluşturma Tarihi")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [Display(Name = "Son Güncellenme")]
    public DateTime? UpdatedDate { get; set; }

    [Display(Name = "Görüntülenme Sayısı")]
    public int ViewCount { get; set; } = 0;

    [Display(Name = "Yorum Sayısı")]
    public int CommentCount { get; set; } = 0;

    [Display(Name = "Yayın Durumu")]
    public bool IsPublished { get; set; } = true;

    public virtual Forum? Forum { get; set; }
    public virtual ICollection<Comment>? Comments { get; set; }
}
