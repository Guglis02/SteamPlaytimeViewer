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

    public string Description => "Synchronize game data (usage: sync account|local)";

    public async Task<bool> HandleAsync(string[] args, AppState state)
    {
        if (args.Length == 0 || args.Length > 1)
        {
            state.StatusMessage = "[yellow]Use: sync account (API) or sync local (VDF file)[/]";
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
            state.StatusMessage = $"[red]Invalid sync type '{syncType}'. Use 'account' or 'local'.[/]";
            return false;
        }
    }

    private async Task<bool> SyncAccountAsync(AppState state)
    {
        var steamId = state.CurrentUser.SteamId;

        if (string.IsNullOrWhiteSpace(steamId))
        {
            state.StatusMessage = $"[red]Cannot sync '{state.CurrentUser.Username}': SteamID not found in database.[/]";
            return false;
        }

        try
        {
            state.StatusMessage = "[cyan]Syncing account data from Steam API...[/]";
            state.MarkDirty();

            await _steamSyncService.SyncUserDataAsync(steamId);

            state.StatusMessage = $"[green]Sincronização da conta concluída![/]";
            state.ShouldUpdateList = true;

            return true;
        }
        catch (Exception ex)
        {
            state.StatusMessage = $"[red]Sync error: {ex.Message}[/]";
            return false;
        }
    }

    private async Task<bool> SyncLocalAsync(AppState state)
    {
        if (string.IsNullOrWhiteSpace(state.SteamFolder))
        {
            state.StatusMessage = "[red]Steam folder not configured. Use 'folder <path>' to set it.[/]";
            return false;
        }

        var steamId = state.CurrentUser.SteamId;

        if (string.IsNullOrWhiteSpace(steamId))
        {
            state.StatusMessage = $"[red]Cannot sync '{state.CurrentUser.Username}': SteamID not found in database.[/]";
            return false;
        }

        try
        {
            state.StatusMessage = "[cyan]Syncing local VDF data...[/]";
            state.MarkDirty();

            if (!await _localVdfService.SyncLocalLibraryAsync(state.CurrentUser, state.SteamFolder))
            {
                return false;
            }

            state.StatusMessage = "[green]Local sync completed successfully![/]";
            state.ShouldUpdateList = true;

            return true;
        }
        catch (Exception ex)
        {
            state.StatusMessage = $"[red]Sync error: {ex.Message}[/]";
            return false;
        }
    }
}