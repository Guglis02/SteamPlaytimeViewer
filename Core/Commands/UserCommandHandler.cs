using SteamPlaytimeViewer.Services;

namespace SteamPlaytimeViewer.Core.Commands;

public class UserCommandHandler : ICommandHandler
{
    private readonly DataService _dataService;

    public UserCommandHandler(DataService dataService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
    }

    public string Description => "Muda o usuário (uso: user <username|steamid>)";

    public async Task<bool> HandleAsync(string[] args, AppState state)
    {
        if (args.Length == 0)
        {
            state.StatusMessage = "[yellow]Use: user <username> ou user <steamid>[/]";
            return false;
        }

        var userInput = string.Join(" ", args).Trim();
        UserInfo? userInfo = null;

        // Tentar resolver como SteamID
        if (ulong.TryParse(userInput, out _) && userInput.Length >= 17)
        {
            try
            {
                userInfo = await _dataService.ResolveBySteamIdAsync(userInput);
                
                if (userInfo == null)
                {
                    state.StatusMessage = $"[red]SteamID '{userInput}' não encontrado na Steam API.[/]";
                    return false;
                }
            }
            catch (Exception ex)
            {
                state.StatusMessage = $"[red]Erro: {ex.Message}[/]";
                return false;
            }
        }
        else
        {
            // Tentar resolver como username
            string? steamId = await _dataService.GetSteamIdByUsernameAsync(userInput);

            if (steamId == null)
            {
                state.StatusMessage = 
                    $"[yellow]Usuário '{userInput}' não encontrado. Use o SteamID para registrar um novo usuário.[/]";
                return false;
            }

            userInfo = new UserInfo(steamId, userInput);
        }

        // Atualizar estado
        state.CurrentUser = userInfo!;
        state.ScrollIndex = 0;
        state.SearchQuery = null; // Limpar busca anterior
        state.SortColumn = nameof(GameView.Title); // Reset ordenação
        state.SortAscending = true;
        state.ShouldUpdateList = true;

        state.StatusMessage = $"[green]Usuário alterado para {userInfo.Username}![/]";
        state.MarkDirty();

        return true;
    }
}