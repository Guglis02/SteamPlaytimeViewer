using Microsoft.EntityFrameworkCore;
using SteamPlaytimeViewer.Models;

namespace SteamPlaytimeViewer.Data;

public class SteamDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<UserGameStats> UserGameStats { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=steamdata.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserGameStats>()
            .HasKey(ugs => new { ugs.UserSteamId, ugs.GameAppId });

        modelBuilder.Entity<UserGameStats>()
            .HasOne(ugs => ugs.User)
            .WithMany(u => u.GameStats)
            .HasForeignKey(ugs => ugs.UserSteamId);

        modelBuilder.Entity<UserGameStats>()
            .HasOne(ugs => ugs.Game)
            .WithMany()
            .HasForeignKey(ugs => ugs.GameAppId);
    }
}