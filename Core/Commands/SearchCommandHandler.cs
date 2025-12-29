namespace SteamPlaytimeViewer.Core.Commands;

public class SearchCommandHandler : ICommandHandler
{
    public string Description => "Busca jogos pelo título (uso: search <termo>) usar sem parâmetro remove o filtro.";

    public async Task<bool> HandleAsync(string[] args, AppState state)
    {
        if (args.Length == 0)
        {
            state.SearchQuery = null;
            state.StatusMessage = "[green]Busca limpa![/]";
        }
        else
        {
            var query = string.Join(" ", args);
            state.SearchQuery = query;
            state.StatusMessage = $"[green]Buscando por '{query}'...[/]";
        }

        state.ShouldUpdateList = true;

        return true;
    }
}