using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public sealed class Context : DbContext
{
    public const string DbName = "115_uwp.db";

    public static string DbPath = SetSavePath();

    public DbSet<ActorInfo> ActorInfos { get; set; } = null!;

    public DbSet<ActorName> ActorNames { get; set; } = null!;

    public DbSet<ActorVideo> ActorVideos { get; set; } = null!;

    public DbSet<Bwh> Bwhs { get; set; } = null!;

    public DbSet<DownHistory> DownHistories { get; set; } = null!;

    public DbSet<FailListIsLikeLookLater> FailListIsLikeLookLater { get; set; } = null!;

    public DbSet<FileToInfo> FileToInfos { get; set; } = null!;

    public DbSet<FilesInfo> FilesInfos { get; set; } = null!;

    public DbSet<IsWm> IsWms { get; set; } = null!;

    public DbSet<ProducerInfo> ProducerInfos { get; set; } = null!;

    public DbSet<SearchHistory> SearchHistories { get; set; } = null!;

    public DbSet<SpiderLog> SpiderLogs { get; set; } = null!;

    public DbSet<SpiderTask> SpiderTasks { get; set; } = null!;

    public DbSet<VideoInfo> VideoInfos { get; set; } = null!;

    private static Context? _context;

    public static string SetSavePath(string? savePath = null)
    {
        savePath ??= Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var path = Path.Combine(savePath, DbName);
        if (DbPath == path) return path;

        _context = null;
        DbPath = path;
        return path;
    }

    public static Context Instance
    {
        get
        {
            if (_context != null) return _context;

            _context = new Context();
            return _context;
        }
    }

    public Context()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options
            .EnableSensitiveDataLogging()
            .UseSqlite($"Data Source={DbPath}");
}