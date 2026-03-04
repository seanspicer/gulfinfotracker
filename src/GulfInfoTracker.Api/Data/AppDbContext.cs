using GulfInfoTracker.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GulfInfoTracker.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<ArticleTopic> ArticleTopics => Set<ArticleTopic>();
    public DbSet<SourcePollLog> SourcePollLogs => Set<SourcePollLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Article
        modelBuilder.Entity<Article>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasIndex(a => a.SourceUrl).IsUnique();
            e.HasIndex(a => a.IngestedAt);
            e.HasIndex(a => a.CredibilityScore);
            e.HasIndex(a => a.Country);
            e.HasIndex(a => a.PluginId);
            e.Property(a => a.NamedEntitiesJson).HasColumnType("text");
            e.Property(a => a.CredibilityReasoning).HasColumnType("text");
            e.Property(a => a.SummaryEn).HasColumnType("text");
            e.Property(a => a.SummaryAr).HasColumnType("text");
        });

        // ArticleTopic (composite PK join table)
        modelBuilder.Entity<ArticleTopic>(e =>
        {
            e.HasKey(at => new { at.ArticleId, at.TopicId });
            e.HasOne(at => at.Article)
             .WithMany(a => a.ArticleTopics)
             .HasForeignKey(at => at.ArticleId);
            e.HasOne(at => at.Topic)
             .WithMany(t => t.ArticleTopics)
             .HasForeignKey(at => at.TopicId);
        });

        // Topic - seed T1–T5
        modelBuilder.Entity<Topic>().HasData(
            new Topic { Id = "T1", LabelEn = "Politics & Government",   LabelAr = "السياسة والحكومة" },
            new Topic { Id = "T2", LabelEn = "Economy & Finance",       LabelAr = "الاقتصاد والمالية" },
            new Topic { Id = "T3", LabelEn = "Energy & Oil",            LabelAr = "الطاقة والنفط" },
            new Topic { Id = "T4", LabelEn = "Business & Investment",   LabelAr = "الأعمال والاستثمار" },
            new Topic { Id = "T5", LabelEn = "Iran / Israel / US Conflict", LabelAr = "الصراع الإيراني / الإسرائيلي / الأمريكي" }
        );
    }
}
