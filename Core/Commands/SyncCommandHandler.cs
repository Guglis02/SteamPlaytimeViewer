using SteamPlaytimeViewer.Services;

namespace SteamPlaytimeViewer.Core.Commands;

public class SyncCommandHandler : ICommandHandler
{
    private readonly SteamSyncService _steamSyncService;
    private readonly LocalVdfService _localVdfService;

    public SyncCommandHandler(
        SteamSyncService steamSyncService,
        LocalVdfService localVdfService)
    {
        _steamSyncService = steamSyncService ?? throw new ArgumentNullException(nameof(steamSyncService));
        _localVdfService = localVdfService ?? throw new ArgumentNullException(nameof(localVdfService));
    }

    public string Description => "Sincroniza dados de jogos (uso: sync account|local)";

    public async Task<bool> HandleAsync(string[] args, AppState state)
    {
        if (args.Length == 0)
        {
            state.StatusMessage = 
                "[yellow]Use: sync account - Sincroniza da API Steam; sync local - Sincroniza de arquivo VDF local[/]";
            return false;
        }

        var syncType = args[0].ToLower();

        if (syncType == "account")
        {
            return await SyncAccountAsync(state);
        }
        else if (syncType == "local")
        {
            return await SyncLocalAsync(state);
        }
        else
        {
            state.StatusMessage = $"[red]Tipo de sincronização '{syncType}' inválido. Use 'account' ou 'local'.[/]";
            return false;
        }
    }

    private async Task<bool> SyncAccountAsync(AppState state)
    {
        var steamId = state.CurrentUser.SteamId;

        if (string.IsNullOrWhiteSpace(steamId))
        {
            state.StatusMessage = 
                $"[red]Não é possível sincronizar '{state.CurrentUser.SteamId}' pela API.[/]\n" +
                $"[yellow]Razão: SteamID não encontrado no banco de dados.[/]";
            return false;
        }

        try
        {
            state.StatusMessage = "[cyan]Sincronizando dados da conta Steam...[/]";
            state.MarkDirty();

            await _steamSyncService.SyncUserDataAsync(steamId);

            state.StatusMessage = $"[green]Sincronização da conta concluída![/]";
            state.ShouldUpdateList = true;

            return true;
        }
        catch (Exception ex)
        {
            state.StatusMessage = $"[red]Erro ao sincronizar: {ex.Message}[/]";
            return false;
        }
    }

    private async Task<bool> SyncLocalAsync(AppState state)
    {
        if (string.IsNullOrWhiteSpace(state.SteamFolder))
        {
            state.StatusMessage = 
                "[red]Pasta do Steam não foi configurada.[/]\n" +
                "[yellow]A detecção automática falhou. Configure manualmente se possível.[/]";
            return false;
        }

        var steamId = state.CurrentUser.SteamId;

        if (string.IsNullOrWhiteSpace(steamId))
        {
            state.StatusMessage = 
                $"[red]Não é possível sincronizar '{state.CurrentUser.SteamId}' localmente.[/]\n" +
                $"[yellow]Razão: SteamID não encontrado no banco de dados.[/]";
            return false;
        }

        try
        {
            state.StatusMessage = "[cyan]Sincronizando dados do VDF local...[/]";
            state.MarkDirty();

            if (!await _localVdfService.SyncLocalLibraryAsync(state.CurrentUser, state.SteamFolder))
            {
                return false;
            }

            state.StatusMessage = $"[green]Sincronização local concluída![/]";
            state.ShouldUpdateList = true;

            return true;
        }
        catch (Exception ex)
        {
            state.StatusMessage = $"[red]Erro ao sincronizar: {ex.Message}[/]";
            return false;
        }
    }
}