namespace freddit2._0.Models;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; }
    public int Upvotes { get; set; }
    public int Downvotes { get; set; }
    public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public bool IsUrl { get; set; }
    public User? User { get; set; }
    
    public int PostId { get; set; }
    public Comment(string content = "", int upvotes = 0, int downvotes = 0, User user = null)
    {
        Content = content;
        Upvotes = upvotes;
        Downvotes = downvotes;
        User = user;
    }
    public Comment() {
        Id = 0;
        Content = "";
        Upvotes = 0;
        Downvotes = 0;
    }
}