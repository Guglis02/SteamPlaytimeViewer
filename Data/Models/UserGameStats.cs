namespace SteamPlaytimeViewer.Models;

public class UserGameStats
{
    public string UserSteamId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public int GameAppId { get; set; }
    public Game Game { get; set; } = null!;

    public int UnlockedAchievements { get; set; } 
    
    public double PlaytimeHours { get; set; }

    public DateTime? FirstPlayed { get; set; } 
    
    public DateTime? LastPlayed { get; set; }
}