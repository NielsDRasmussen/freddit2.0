namespace shared.Model;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public int Upvotes { get; set; }
    public int Downvotes { get; set; }
    public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public bool IsUrl { get; set; }
    public User User { get; set; } = null!;
    public List<Comment> Comments { get; set; } = [];
}
