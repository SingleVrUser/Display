using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context;

public abstract class BaseContext : DbContext
{
    private const string DbName = "115_uwp.db";

    private static string _dbPath = SetSavePath();

    private static string SetSavePath(string? savePath = null)
    {
        savePath ??= Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var path = Path.Combine(savePath, DbName);
        if (_dbPath == path) return path;

        _dbPath = path;
        return path;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options
            .UseSnakeCaseNamingConvention() // 使用Snake case命名法
            .EnableSensitiveDataLogging() // 出BUG时定位到具体那一行，调试时用
            .UseSqlite($"Data Source={_dbPath}");
}