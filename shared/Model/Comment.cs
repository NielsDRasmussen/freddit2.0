namespace shared.Model;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = "";
    public int Upvotes { get; set; }
    public int Downvotes { get; set; }
    public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public bool IsUrl { get; set; }
    public User? User { get; set; }
    public int PostId { get; set; }
}
