using System.ComponentModel.DataAnnotations;

namespace SteamPlaytimeViewer.Models;

public class User
{
    [Key]
    public string SteamId { get; set; } = string.Empty;

    public string Nickname { get; set; } = string.Empty;

    public List<UserGameStats> GameStats { get; set; } = new();
}