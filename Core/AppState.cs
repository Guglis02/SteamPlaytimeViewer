using System.Text;

namespace SteamPlaytimeViewer.Core;

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

    public readonly int TerminalMinSize = 15;
    public int ItemsPerPage => Math.Max(TerminalHeight - TerminalMinSize, TerminalMinSize);
    
    public List<Game> VisibleGames => AllGames
        .Skip(ScrollIndex)
        .Take(ItemsPerPage)
        .ToList();
    
    public void MarkDirty() => IsDirty = true;
    public void ClearDirty() => IsDirty = false;
}