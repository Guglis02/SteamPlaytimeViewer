namespace SteamPlaytimeViewer.Core.Commands;

public class SortCommandHandler : ICommandHandler
{
    public string Description => "Sort games by column (usage: sort <column> [asc|desc])";

    public async Task<bool> HandleAsync(string[] args, AppState state)
    {
        if (args.Length == 0)
        {
            state.StatusMessage = "[yellow]Usage: sort <column> [[asc|desc]]![/] [gray]Columns: title, playtime, achievements, percentage, firstsession, lastsession[/]";
            return false;
        }

        var column = args[0];
        var direction = args.Length > 1 ? args[1] : "asc";

        var validColumns = new[] { "title", "playtime", "achievements", "percentage", "firstsession", "lastsession" };
        if (!validColumns.Contains(column.ToLower()))
        {
            state.StatusMessage = $"[red]Invalid column '{column}'![/]";
            return false;
        }

        state.SortColumn = column;
        state.SortAscending = !direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
        state.ShouldUpdateList = true;

        state.StatusMessage = $"[green]Sorted by {column} ({(state.SortAscending ? "ascending" : "descending")})![/]";
        return true;
    }
}