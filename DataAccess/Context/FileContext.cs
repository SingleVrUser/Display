using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context;

public class FileContext : BaseContext
{
    public DbSet<Files> Files { get; set; } = null!;

}