using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context;

public class SearchHistoryContext : BaseContext
{
    private DbSet<SearchHistory> _searchHistories = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SearchHistory>().ToTable("search_histories");

        modelBuilder.Entity<SearchHistory>(b =>
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
}   