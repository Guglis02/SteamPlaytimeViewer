using System.Text;

namespace SteamPlaytimeViewer;

public class AppState
{
    public string CurrentUser { get; set; } = "Guglis";
    public StringBuilder InputBuffer { get; set; } = new();
    public string StatusMessage { get; set; }
    public int ScrollIndex { get; set; }
    public int TerminalHeight { get; set; }
    public List<Game> AllGames { get; set; } = new();
    public bool IsDirty { get; set; } = true;    

    public AppState(string helpMessage)
    {
        StatusMessage = helpMessage;
    }

    public int ItemsPerPage => TerminalHeight - 15;
    
    public List<Game> VisibleGames => AllGames
        .Skip(ScrollIndex)
        .Take(ItemsPerPage)
        .ToList();
    
    public void MarkDirty() => IsDirty = true;
    public void ClearDirty() => IsDirty = false;
}