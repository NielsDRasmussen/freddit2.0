
using System.Text.Json.Serialization;
using freddit2._0.Data;
using freddit2._0.Models;
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


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Users.Any(u => u.Id == 1))
    {
        db.Users.Add(new User
        {
            Id = 1,
            Username = "TestUser"
        });

        db.SaveChanges();
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
        .OrderByDescending(p => p.Id)
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
app.MapPost("/api/posts", async (Post post, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(1);

    if (user == null)
    {
        return Results.BadRequest("Brugeren blev ikke fundet.");
    }

    post.User = user;
    post.Id = 0;

    db.Posts.Add(post);
    await db.SaveChangesAsync();

    return Results.Created($"/api/posts/{post.Id}", post);
});

// POST: Opret en kommentar
app.MapPost("/api/posts/{id}/comments",
    async (int id, Comment comment, AppDbContext db) =>
    {
        var post = await db.Posts.FindAsync(id);

        if (post == null)
        {
            return Results.NotFound("Opslaget blev ikke fundet.");
        }

        var user = await db.Users.FindAsync(1);

        if (user == null)
        {
            return Results.BadRequest("Brugeren blev ikke fundet.");
        }

        comment.Id = 0;
        comment.PostId = id;
        comment.User = user;

        db.Comments.Add(comment);
        await db.SaveChangesAsync();

        return Results.Created(
            $"/api/posts/{id}/comments/{comment.Id}",
            comment);
    });

app.Run();
