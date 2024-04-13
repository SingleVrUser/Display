using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models.Entity;

[Keyless]
public class FileVideo
{
    public long FileId { get; set; }
    
    public long VideoId { get; set; }
}
