using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models;

public class Post
{
    public int Id { get; set; }
    public List<Tag> Tags { get; } = [];
    
    public int Name { get; set; }
}

public class Tag
{    public int Id { get; set; }
    public List<Post> Posts { get; } = [];
    public string Name { get; set; }
}

[Keyless]
public class PostTag
{
    public int PostsId { get; set; }
    public int TagsId { get; set; }
    
    public string Name { get; set; }
}