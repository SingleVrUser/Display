using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context;

public class FileSpiderResultContext : BaseContext
{
    public DbSet<FileSpiderResult> FileSpiderResults { get; set; } = null!;
}