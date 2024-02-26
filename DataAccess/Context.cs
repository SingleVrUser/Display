using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class Context : DbContext
{
    private const string DbName = "115_uwp.db";

    public static string DbPath = SetSavePath();

    public virtual DbSet<ActorInfo> ActorInfos { get; set; } = null!;

    public virtual DbSet<ActorName> ActorNames { get; set; } = null!;

    public virtual DbSet<ActorVideo> ActorVideos { get; set; } = null!;

    public virtual DbSet<Bwh> Bwhs { get; set; } = null!;

    public virtual DbSet<DownHistory> DownHistories { get; set; } = null!;

    public virtual DbSet<FailListIslikeLookLater> FailListIsLikeLookLater { get; set; } = null!;

    public virtual DbSet<FileToInfo> FileToInfos { get; set; } = null!;

    public virtual DbSet<FilesInfo> FilesInfos { get; set; } = null!;

    public virtual DbSet<IsWm> IsWms { get; set; } = null!;

    public virtual DbSet<ProducerInfo> ProducerInfos { get; set; } = null!;

    public virtual DbSet<SearchHistory> SearchHistories { get; set; } = null!;

    public virtual DbSet<SpiderLog> SpiderLogs { get; set; } = null!;

    public virtual DbSet<SpiderTask> SpiderTasks { get; set; } = null!;

    public virtual DbSet<VideoInfo> VideoInfos { get; set; } = null!;

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
        set => _context = value;
    }


    private Context()
    {
    }


    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}