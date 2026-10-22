
using System.Text.Json.Serialization;
using freddit2._0.Data;
using shared.Model;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

//Seed Data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var testUser = await db.Users.FirstOrDefaultAsync(u => u.Id == 1);
    if (testUser == null)
    {
        testUser = new User { Id = 1, Username = "TestUser" };
        db.Users.Add(testUser);
        await db.SaveChangesAsync();
    }

    if (!await db.Posts.AnyAsync())
    {
        var post = new Post
        {
            Title = "Welcome to Freddit",
            Content = "This is a sample post. Add a comment or try voting.",
            User = testUser,
            CreatedAt = DateOnly.FromDateTime(DateTime.Today)
        };

        db.Posts.Add(post);
        await db.SaveChangesAsync();

        db.Comments.Add(new Comment
        {
            Content = "This is a sample comment.",
            User = testUser,
            PostId = post.Id,
            CreatedAt = DateOnly.FromDateTime(DateTime.Today)
        });

        await db.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazor");

// GET: Hent alle opslag
app.MapGet("/api/posts", async (AppDbContext db) =>
{
    return await db.Posts
        .Include(p => p.User)
        .Include(p => p.Comments)
            .ThenInclude(c => c.User)
        .OrderByDescending(p => p.CreatedAt)
        .ToListAsync();
});

// GET: Hent ét opslag
app.MapGet("/api/posts/{id}", async (int id, AppDbContext db) =>
{
    var post = await db.Posts
        .Include(p => p.User)
        .Include(p => p.Comments)
            .ThenInclude(c => c.User)
        .FirstOrDefaultAsync(p => p.Id == id);

    if (post == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(post);
});

// PUT: Upvote et opslag
app.MapPut("/api/posts/{id}/upvote", async (int id, AppDbContext db) =>
{
    var post = await db.Posts.FindAsync(id);

    if (post == null)
    {
        return Results.NotFound();
    }

    post.Upvotes++;
    await db.SaveChangesAsync();

    return Results.Ok(post);
});

// PUT: Downvote et opslag
app.MapPut("/api/posts/{id}/downvote", async (int id, AppDbContext db) =>
{
    var post = await db.Posts.FindAsync(id);

    if (post == null)
    {
        return Results.NotFound();
    }

    post.Downvotes++;
    await db.SaveChangesAsync();

    return Results.Ok(post);
});

// PUT: Upvote en kommentar
app.MapPut("/api/posts/{postid}/comments/{commentid}/upvote",
    async (int postid, int commentid, AppDbContext db) =>
    {
        var comment = await db.Comments
            .FirstOrDefaultAsync(c => c.Id == commentid && c.PostId == postid);

        if (comment == null)
        {
            return Results.NotFound();
        }

        comment.Upvotes++;
        await db.SaveChangesAsync();

        return Results.Ok(comment);
    });

// PUT: Downvote en kommentar
app.MapPut("/api/posts/{postid}/comments/{commentid}/downvote",
    async (int postid, int commentid, AppDbContext db) =>
    {
        var comment = await db.Comments
            .FirstOrDefaultAsync(c => c.Id == commentid && c.PostId == postid);

        if (comment == null)
        {
            return Results.NotFound();
        }

        comment.Downvotes++;
        await db.SaveChangesAsync();

        return Results.Ok(comment);
    });

// POST: Opret et opslag
app.MapPost("/api/posts", async (CreatePostRequest request, AppDbContext db) =>
{
    var title = request.Title?.Trim();
    var content = request.Content?.Trim();
    var authorName = request.AuthorName?.Trim();

    if (string.IsNullOrWhiteSpace(title) ||
        string.IsNullOrWhiteSpace(content) ||
        string.IsNullOrWhiteSpace(authorName))
    {
        return Results.BadRequest("Titel, indhold og forfatternavn skal udfyldes.");
    }

    if (request.IsUrl)
    {
        bool validUrl =
            Uri.TryCreate(content, UriKind.Absolute, out var url) &&
            (url.Scheme == "http" || url.Scheme == "https");

        if (!validUrl)
        {
            return Results.BadRequest("Indtast en gyldig URL, der starter med http:// eller https://.");
        }
    }

    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == authorName);
    
    if (user == null)
    {
        user = new User { Username = authorName };
        db.Users.Add(user);
    }

    var post = new Post
    {
        Title = title,
        Content = content,
        User = user,
        IsUrl = request.IsUrl,
        CreatedAt = DateOnly.FromDateTime(DateTime.Now)
    };

    db.Posts.Add(post);
    await db.SaveChangesAsync();

    return Results.Created($"/api/posts/{post.Id}", post);
});

// POST: Opret en kommentar
app.MapPost("/api/posts/{id}/comments",
    async (int id, CreateCommentRequest request, AppDbContext db) =>
    {
        var post = await db.Posts.FindAsync(id);

        if (post == null)
        {
            return Results.NotFound("Opslaget blev ikke fundet.");
        }

        var content = request.Content?.Trim();
        var authorName = request.AuthorName?.Trim();

        if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(authorName))
        {
            return Results.BadRequest("Kommentar og forfatternavn skal udfyldes.");
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == authorName);
        if (user == null)
        {
            user = new User { Username = authorName };
            db.Users.Add(user);
        }

        var comment = new Comment
        {
            Content = content,
            PostId = id,
            User = user,
            CreatedAt = DateOnly.FromDateTime(DateTime.Now)
        };

        db.Comments.Add(comment);
        await db.SaveChangesAsync();

        return Results.Created(
            $"/api/posts/{id}/comments/{comment.Id}",
            comment);
    });

app.Run();


public record CreatePostRequest(string Title, string Content, string AuthorName, bool IsUrl);
public record CreateCommentRequest(string Content, string AuthorName);
