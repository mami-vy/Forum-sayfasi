using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using mym.Models;

namespace mym.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Forum> Forums { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<LoginActivity> LoginActivities { get; set; }
    public DbSet<AdminNotification> AdminNotifications { get; set; }
    public DbSet<ThemeSetting> ThemeSettings { get; set; }
    public DbSet<SiteSetting> SiteSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Forum - Topic ilişkisi
        builder.Entity<Forum>()
            .HasMany(f => f.Topics)
            .WithOne(t => t.Forum)
            .HasForeignKey(t => t.ForumId)
            .OnDelete(DeleteBehavior.Cascade);

        // Topic - Comment ilişkisi
        builder.Entity<Topic>()
            .HasMany(t => t.Comments)
            .WithOne(c => c.Topic)
            .HasForeignKey(c => c.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<LoginActivity>()
            .HasIndex(x => x.CreatedAtUtc);

        builder.Entity<LoginActivity>()
            .Property(x => x.Username)
            .HasMaxLength(128);

        builder.Entity<LoginActivity>()
            .Property(x => x.Description)
            .HasMaxLength(256);

        builder.Entity<AdminNotification>()
            .Property(x => x.Title)
            .HasMaxLength(128);

        builder.Entity<AdminNotification>()
            .Property(x => x.Message)
            .HasMaxLength(300);

        // Seed data - Forum kategorileri
        builder.Entity<Forum>().HasData(
            new Forum
            {
                Id = 1,
                Title = "Dersler & Ödevler",
                Description = "Tüm dersler hakkında soru sor, ödevlerde yardım al",
                Category = "Akademik",
                Icon = "fas fa-book",
                CreatedDate = new DateTime(2026, 2, 11, 21, 29, 26, DateTimeKind.Local).AddTicks(9643)
            },
            new Forum
            {
                Id = 2,
                Title = "Kampüs Etkinlikleri",
                Description = "Sosyal etkinlikler, festivaller ve kulüp haberleri",
                Category = "Sosyal",
                Icon = "fas fa-calendar-alt",
                CreatedDate = new DateTime(2026, 2, 11, 21, 29, 26, DateTimeKind.Local).AddTicks(9876)
            },
            new Forum
            {
                Id = 3,
                Title = "Sosyal & Tanışma",
                Description = "Arkadaşlar bul ve öğrenci arkadaşlığı kur",
                Category = "Sosyal",
                Icon = "fas fa-users",
                CreatedDate = new DateTime(2026, 2, 11, 21, 29, 26, DateTimeKind.Local).AddTicks(9879)
            },
            new Forum
            {
                Id = 4,
                Title = "Kariyer & İnternship",
                Description = "İnternship fırsatları ve kariyer danışması",
                Category = "Kariyer",
                Icon = "fas fa-briefcase",
                CreatedDate = new DateTime(2026, 2, 11, 21, 29, 26, DateTimeKind.Local).AddTicks(9881)
            },
            new Forum
            {
                Id = 5,
                Title = "Konut & Yaşam",
                Description = "Yurt hayatı ve apartman paylaşımı",
                Category = "Yaşam",
                Icon = "fas fa-home",
                CreatedDate = new DateTime(2026, 2, 11, 21, 29, 26, DateTimeKind.Local).AddTicks(9884)
            },
            new Forum
            {
                Id = 6,
                Title = "Hobi & Eğlence",
                Description = "Oyunlar, filmler, müzik ve diğer hobi paylaşımları",
                Category = "Eğlence",
                Icon = "fas fa-gamepad",
                CreatedDate = new DateTime(2026, 2, 11, 21, 29, 26, DateTimeKind.Local).AddTicks(9886)
            }
        );
    }
}
