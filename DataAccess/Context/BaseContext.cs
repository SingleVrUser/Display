

using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;
using FileInfo = DataAccess.Models.Entity.FileInfo;

namespace DataAccess.Context;

public class BaseContext : DbContext
{
    public DbSet<FileInfo> FileInfo { get; set; } = null!;
    
    public DbSet<VideoInfo> VideoInfo { get; set; } = null!;

    public DbSet<DownHistory> DownHistory { get; set; } = null!;
    
    public DbSet<DirectorInfo> DirectorInfo { get; set; } = null!;
    
    public DbSet<ProducerInfo> ProducerInfo { get; set; } = null!;
    
    public DbSet<PublisherInfo> PublisherInfo { get; set; } = null!;
    
    public DbSet<SeriesInfo> SeriesInfo { get; set; } = null!;

    public DbSet<VideoInterest> VideoInterest { get; set; } = null!;
    
    public DbSet<ActorInfo> ActorInfo { get; set; } = null!;
    
    public DbSet<ActorName> ActorName { get; set; } = null!;
    
    // public DbSet<ActorVideo> ActorVideo { get; set; } = null!;
    
    public DbSet<CategoryInfo> Category { get; set; } = null!;
    
    public DbSet<ActorInterest> ActorInterest { get; set; } = null!;
    
    private const string DbName = "115_uwp.db";

    private static string? _dbPath;

    private static string DbPath => _dbPath ?? SetSavePath();

    public static string SetSavePath(string? savePath = null)
    {
        savePath ??= Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var path = Path.Combine(savePath, DbName);
        if (_dbPath == path) return path;

        _dbPath = path;
        return path;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 多对多
        modelBuilder.Entity<VideoInfo>()
            .HasMany(e => e.ActorInfoList)
            .WithMany(e => e.VideoInfos)
            // 中间表
            .UsingEntity<ActorVideo>();

        modelBuilder.Entity<VideoInfo>()
            .HasMany(e => e.CategoryList)
            .WithMany(e => e.VideoInfos)
            // 中间表
            .UsingEntity<CategoryVideo>();

        // TODO  标签页的多对多
        
        // 搜索历史
        //modelBuilder.Entity<SearchHistory>().ToTable("search_histories");
        //modelBuilder.Entity<SearchHistory>(b =>
        //    {
        //        b.Property(t => t.UpdateTime)
        //            .HasColumnType("DATETIME")
        //            .HasColumnType("DATETIME")
        //            .HasDefaultValueSql("datetime('now')")
        //            .HasColumnName("create_time");
        //        b.Property(t => t.CreateTime)
        //            .HasColumnType("DATETIME")
        //            .HasDefaultValueSql("datetime('now')")
        //            .HasColumnName("update_time");
        //    }
        //);

        ConfigureTime<SearchHistory>(modelBuilder);
        ConfigureTime<ActorInfo>(modelBuilder);
        ConfigureTime<BwhInfo>(modelBuilder);
        ConfigureTime<CategoryInfo>(modelBuilder);
        ConfigureTime<DirectorInfo>(modelBuilder);
        ConfigureTime<DownHistory>(modelBuilder);
        ConfigureTime<FileInfo>(modelBuilder);
        ConfigureTime<ProducerInfo>(modelBuilder);
        ConfigureTime<PublisherInfo>(modelBuilder);
        ConfigureTime<SearchHistory>(modelBuilder);
        ConfigureTime<SeriesInfo>(modelBuilder);
        ConfigureTime<VideoInfo>(modelBuilder);
        ConfigureTime<VideoInterest>(modelBuilder);

    }

    private static void ConfigureTime<T>(ModelBuilder modelBuilder) where T : BaseEntity
    {
        modelBuilder.Entity<T>(b =>
            {
                b.Property(t => t.UpdateTime)
                    .HasColumnType("DATETIME")
                    .HasColumnType("DATETIME")
                    .HasDefaultValueSql("datetime('now')")
                    .HasColumnName("create_time");
                b.Property(t => t.CreateTime)
                    .HasColumnType("DATETIME")
                    .HasDefaultValueSql("datetime('now')")
                    .HasColumnName("update_time");
            }
        );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options
            .UseSnakeCaseNamingConvention() // 使用Snake case命名法
            .EnableSensitiveDataLogging() // 出BUG时定位到具体那一行，调试时用
            .UseSqlite($"Data Source={DbPath}");
}