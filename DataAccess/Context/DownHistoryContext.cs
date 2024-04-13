using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context;

public class DownHistoryContext : BaseContext
{
    public DbSet<DownHistory> DownHistories { get; set; } = null!;
}