using Microsoft.EntityFrameworkCore;
using NetScraper.Api;
using NetScraper.Api.Data;
using NetScraper.Api.Models;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// ---------- Config ----------
bool useInMemory = builder.Configuration.GetValue<bool>("UseInMemory", false);

string? connStr =
    builder.Configuration.GetConnectionString("Default")
    ?? Environment.GetEnvironmentVariable("SQL_CONN_STR");

string frontendOrigin =
    builder.Configuration["FrontendOrigin"]
    ?? Environment.GetEnvironmentVariable("FRONTEND_ORIGIN")
    ?? "http://localhost:5173";

// ---------- Services ----------
if (useInMemory)
{
    builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("devdb"));
}
else
{
    if (string.IsNullOrWhiteSpace(connStr))
        throw new InvalidOperationException("Missing connection string. Set ConnectionStrings:Default or SQL_CONN_STR.");

    builder.Services.AddDbContext<AppDbContext>(o =>
        o.UseSqlServer(connStr, sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null)));
}

const string CorsPolicy = "FrontendOnly";
builder.Services.AddCors(o =>
{
    o.AddPolicy(CorsPolicy, p =>
        p.WithOrigins(frontendOrigin)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

var app = builder.Build();

// ---------- Middleware ----------
app.UseHttpsRedirection();
app.UseCors(CorsPolicy);

// ---------- Health ----------
app.MapGet("/", () => "OK");
app.MapGet("/health/db", async (AppDbContext db) =>
{
    try
    {
        var can = await db.Database.CanConnectAsync();
        return Results.Ok(new { db = can ? "up" : "down" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// ---------- Endpoints ----------

// Groups
app.MapGet("/api/groups", async (AppDbContext db) =>
    await db.SearchGroups
            .Select(g => new GroupDto(g.Id, g.Name, g.Terms.Count))
            .OrderBy(g => g.Name)
            .ToListAsync());

app.MapPost("/api/groups", async (AppDbContext db, CreateGroupDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Name)) return Results.BadRequest("Name required");
    var g = new SearchGroup { Name = dto.Name.Trim() };
    db.SearchGroups.Add(g);
    await db.SaveChangesAsync();
    return Results.Created($"/api/groups/{g.Id}", new GroupDto(g.Id, g.Name, 0));
});

app.MapDelete("/api/groups/{id:int}", async (AppDbContext db, int id) =>
{
    var g = await db.SearchGroups.FindAsync(id);
    if (g is null) return Results.NotFound();
    db.Remove(g);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Terms
app.MapGet("/api/groups/{groupId:int}/terms", async (AppDbContext db, int groupId) =>
{
    var exists = await db.SearchGroups.AnyAsync(g => g.Id == groupId);
    if (!exists) return Results.NotFound();

    var terms = await db.SearchTerms
        .Where(t => t.SearchGroupId == groupId)
        .OrderBy(t => t.Term)
        .Select(t => new TermDto(t.Id, t.Term, t.SearchGroupId, t.StartDate, t.EndDate, t.OutputQuery))
        .ToListAsync();

    return Results.Ok(terms);
});

app.MapPost("/api/terms", async (AppDbContext db, CreateTermDto dto) =>
{
    var group = await db.SearchGroups.FindAsync(dto.SearchGroupId);
    if (group is null) return Results.BadRequest("Invalid SearchGroupId");

    var t = new SearchTerm
    {
        Term = dto.Term.Trim(),
        SearchGroupId = dto.SearchGroupId,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        OutputQuery = dto.OutputQuery
    };
    db.SearchTerms.Add(t);
    await db.SaveChangesAsync();

    return Results.Created($"/api/terms/{t.Id}",
        new TermDto(t.Id, t.Term, t.SearchGroupId, t.StartDate, t.EndDate, t.OutputQuery));
});

app.MapDelete("/api/terms/{id:int}", async (AppDbContext db, int id) =>
{
    var t = await db.SearchTerms.FindAsync(id);
    if (t is null) return Results.NotFound();
    db.Remove(t);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ---------- Startup tasks ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!useInMemory && db.Database.IsSqlServer())
    {
        // Only try to migrate when using a real SQL provider
        await db.Database.MigrateAsync();
    }
    else if (useInMemory)
    {
        // Seed a little data so your UI isn't empty in dev
        if (!db.SearchGroups.Any())
        {
            var g = new SearchGroup { Name = "Demo Group" };
            db.SearchGroups.Add(g);
            db.SearchTerms.Add(new SearchTerm
            {
                Term = "demo-term",
                SearchGroup = g,
                OutputQuery = "select 1",
                StartDate = DateTime.UtcNow.Date,
                EndDate = DateTime.UtcNow.Date.AddDays(7)
            });
            await db.SaveChangesAsync();
        }
    }
}

app.Run();
