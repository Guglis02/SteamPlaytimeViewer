using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamPlaytimeViewer.Models;

public class Game
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int AppId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int TotalAchievements { get; set; }
}