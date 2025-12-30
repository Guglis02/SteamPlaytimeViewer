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

    /// <summary>
    /// Inicia a sincronização de dados da conta Steam via API oficial de forma assíncrona no background.
    /// Busca todos os jogos e suas estatísticas (tempo de jogo, conquistas) e salva no banco de dados.
    /// Retorna imediatamente sem esperar a sincronização completar.
    /// </summary>
    private async Task<bool> SyncAccountAsync(AppState state)
    {
        var steamId = state.CurrentUser.SteamId;

        if (string.IsNullOrWhiteSpace(steamId))
        {
            state.StatusMessage = $"[red]Cannot sync '{state.CurrentUser.Username}': SteamID not found in database.[/]";
            return false;
        }

        // Inicia o sync em background
        _ = Task.Run(async () =>
        {
            state.IsProcessingCommand = true;

            try
            {
                state.StatusMessage = "[cyan]Syncing account data from Steam API...[/]";

                await _steamSyncService.SyncUserDataAsync(steamId);

                state.StatusMessage = "[green]Sincronização da conta concluída![/]";
                state.ShouldUpdateList = true;
            }
            catch (Exception ex)
            {
                state.StatusMessage = $"[red]Sync error: {ex.Message}[/]";
            }
            finally
            {
                state.IsProcessingCommand = false;
            }
        });

        return true;
    }

    /// <summary>
    /// Inicia a sincronização de dados da biblioteca local via arquivo VDF em background.
    /// Lê os jogos instalados localmente e enriquece com dados da API Steam.
    /// Retorna imediatamente sem esperar a sincronização completar.
    /// </summary>
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

        // Inicia o sync em background
        _ = Task.Run(async () =>
        {
            state.IsProcessingCommand = true;

            try
            {
                state.StatusMessage = "[cyan]Syncing local VDF data...[/]";

                if (!await _localVdfService.SyncLocalLibraryAsync(state.CurrentUser, state.SteamFolder))
                {
                    return;
                }
                else
                {
                    state.StatusMessage = "[green]Local sync completed successfully![/]";
                    state.ShouldUpdateList = true;
                }
            }
            catch (Exception ex)
            {
                state.StatusMessage = $"[red]Sync error: {ex.Message}[/]";
            }
            finally
            {
                state.IsProcessingCommand = false;
            }
        });

        return true; // Retorna imediatamente, não espera o sync
    }
}