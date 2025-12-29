namespace SteamPlaytimeViewer.Core.Commands;

public class SortCommandHandler : ICommandHandler
{
    public string Description => "Ordena os jogos por coluna (uso: sort <coluna> [asc|desc])";

    public async Task<bool> HandleAsync(string[] args, AppState state)
    {
        if (args.Length == 0)
        {
            state.StatusMessage = "[yellow]Use: sort [title|playtime|achievements|percentage|firstsession|lastsession] [asc|desc][/]";
            return false;
        }

        var column = args[0];
        var direction = args.Length > 1 ? args[1] : "asc";

        var validColumns = new[] { "title", "playtime", "achievements", "percentage", "firstsession", "lastsession" };
        if (!validColumns.Contains(column.ToLower()))
        {
            state.StatusMessage = $"[red]Coluna '{column}' inválida![/]";
            return false;
        }

        state.SortColumn = column;
        state.SortAscending = !direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
        state.ShouldUpdateList = true;

        state.StatusMessage = $"[green]Ordenado por {column} ({(state.SortAscending ? "ascendente" : "descendente")})![/]";
        return true;
    }
}