using Microsoft.EntityFrameworkCore;
using NetScraper.Api.Models;

namespace NetScraper.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> opts) : DbContext(opts)
{
    public DbSet<SearchGroup> SearchGroups => Set<SearchGroup>();
    public DbSet<SearchTerm> SearchTerms => Set<SearchTerm>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<SearchGroup>()
         .HasIndex(g => g.Name).IsUnique();

        b.Entity<SearchTerm>()
         .HasOne(t => t.SearchGroup)
         .WithMany(g => g.Terms)
         .HasForeignKey(t => t.SearchGroupId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
