using SteamPlaytimeViewer.Services;

namespace SteamPlaytimeViewer.Core.Commands;

public class UserCommandHandler : ICommandHandler
{
    private readonly DataService _dataService;

    public UserCommandHandler(DataService dataService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
    }

    public string Description => "Muda o usuário/perfil (uso: user <nome>)";

    public async Task<bool> HandleAsync(string[] args, AppState state)
    {
        if (args.Length == 0)
        {
            state.StatusMessage = "[yellow]Use: user <nome>[/]";
            return false;
        }

        var newUser = string.Join(" ", args).Trim();

        if (await _dataService.UserExistsAsync(newUser))
        {
            state.CurrentUser = newUser;
            state.ScrollIndex = 0;
            state.AllGames = await _dataService.GetGamesAsync(newUser);
            state.StatusMessage = $"[green]Usuário alterado para {newUser}![/]";
            return true;
        }

        state.StatusMessage = $"[red]Usuário '{newUser}' não encontrado![/]";
        return false;
    }
}