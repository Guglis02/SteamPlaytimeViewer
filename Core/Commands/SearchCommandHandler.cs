namespace SteamPlaytimeViewer.Core.Commands;

public class SearchCommandHandler : ICommandHandler
{
    public string Description => "Search games by title (usage: search <term>)";

    public async Task<bool> HandleAsync(string[] args, AppState state)
    {
        if (args.Length == 0)
        {
            state.SearchQuery = null;
            state.StatusMessage = "[green]Search cleared.[/]";
        }
        else
        {
            var query = string.Join(" ", args);
            state.SearchQuery = query;
            state.StatusMessage = $"[cyan]Searching for '{query}'...[/]";
        }

        state.ShouldUpdateList = true;

        return true;
    }
}