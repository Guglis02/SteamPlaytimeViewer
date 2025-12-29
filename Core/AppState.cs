using System.Drawing;
using System.Text;

namespace SteamPlaytimeViewer.Core;

public class AppState
{ 
    // Sync
    public UserInfo CurrentUser { get; set; }
    public List<GameView> AllGames { get; set; } = new();
    public string SteamFolder { get; internal set; }
    public bool ShouldSyncAccount { get; set;}
    public bool ShouldSyncLocal { get; set;}

    public bool ShouldUpdateList { get; set; }

    // Search
    public string? SearchQuery { get; set; }
        
    // IO
    public StringBuilder InputBuffer { get; set; } = new();
    public string StatusMessage { get; set; }

    // Terminal Window
    public readonly int TerminalMinSize = 17;
    public int TerminalHeight { get; set; }
    public int TerminalWidth { get; set; }

    // Dirty State
    public bool IsDirty { get; set; } = true;    
    
    // Appearance
    public string MainColor = Color.Teal.ToString();

    // Pagination
    public int ScrollIndex { get; set; }
    public int ItemsPerPage => Math.Max(TerminalHeight - TerminalMinSize, 0);
    public List<GameView> VisibleGames => AllGames
        .Skip(ScrollIndex)
        .Take(ItemsPerPage)
        .ToList();
    
    // Exit
    public bool ShouldExit { get; set; } = false;

    // Sorting
    public string SortColumn { get; set; } = nameof(GameView.Title);
    public bool SortAscending { get; set; } = true;

    public void MarkDirty() => IsDirty = true;
    public void ClearDirty() => IsDirty = false;
    
}

/// <summary>
/// Encapsula informações básicas de um usuário.
/// </summary>
public class UserInfo
{
    public string SteamId { get; set; }
    public string? Username { get; set; }

    public UserInfo(string steamId, string? username = null)
    {
        SteamId = steamId;
        Username = username;
    }
}