using DataAccess.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context;

public class VideoContext : BaseContext
{
    public DbSet<FileVideo> FileToInfos { get; set; } = null!;
    
    public DbSet<FailListIsLikeLookLater> FailListIsLikeLookLater { get; set; } = null!;
    
    public DbSet<Video> VideoInfos { get; set; } = null!;
    
    public DbSet<IsWm> IsWms { get; set; } = null!;
    
    public DbSet<ProducerInfo> ProducerInfos { get; set; } = null!;
}