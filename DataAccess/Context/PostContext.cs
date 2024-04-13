using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context;

public class PostContext : BaseContext
{
    public DbSet<Post> context { get; set; }
}