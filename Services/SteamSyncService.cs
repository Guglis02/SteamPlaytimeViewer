using SteamPlaytimeViewer.Core;
using SteamPlaytimeViewer.Data.Dtos;
using SteamPlaytimeViewer.External.SteamApi;

public class SteamSyncService
{
    private readonly SteamApiConnection _steamApi;
    private readonly IGameRepository _repository;
    private readonly AppState _appState;

    public SteamSyncService(SteamApiConnection steamApi, IGameRepository repository, AppState appState)
    {
        _steamApi = steamApi;
        _repository = repository;
        _appState = appState;
    }

    public async Task SyncUserDataAsync(string steamId)
    {
        _appState.StatusMessage = $"[cyan]Starting sync for {steamId}...[/]";

        var playerSummary = await _steamApi.GetPlayerSummaryAsync(steamId);
        await _repository.SaveUserAsync(steamId, playerSummary.Nickname);

        var ownedGames = await _steamApi.GetOwnedGamesAsync(steamId);

        var playedGames = ownedGames
            .Where(g => g.PlaytimeForeverMinutes > 0)
            .ToList();

        _appState.StatusMessage = $"[cyan]Found {playedGames.Count} games. Fetching details...[/]";

        var gamesToSave = new List<GameImportDto>();
        int count = 0;

        foreach (var game in playedGames)
        {
            count++;
            _appState.StatusMessage = $"[cyan]Processing {count}/{playedGames.Count}: {game.Name}...[/]";

            try
            {
                var stats = await _steamApi.GetGameStatsAsync(steamId, game.AppId);

                var dto = new GameImportDto(
                    game.AppId,
                    game.Name,
                    stats.TotalAchievements,
                    stats.UnlockedAchievements,
                    game.PlaytimeForeverMinutes / 60.0, // Convertendo minutos para horas
                    stats.FirstUnlockedTime,     // Data do primeiro achievement
                    DateTimeOffset.FromUnixTimeSeconds(game.LastPlayedUnix).DateTime
                );

                gamesToSave.Add(dto);
            }
            catch (Exception)
            {
                // Jogos sem achievements dão erro ou retornam vazio.
                _appState.StatusMessage = $"[yellow]Warning: No stats for {game.Name}. Saving basic info.[/]";

                gamesToSave.Add(new GameImportDto(
                    game.AppId,
                    game.Name,
                    0, 0,
                    game.PlaytimeForeverMinutes / 60.0,
                    null,
                    DateTimeOffset.FromUnixTimeSeconds(game.LastPlayedUnix).DateTime
                ));
            }

            await Task.Delay(100);
        }

        _appState.StatusMessage = "[cyan]Saving to database...[/]";
        await _repository.SaveGamesAsync(steamId, gamesToSave);

        _appState.StatusMessage = "[green]Sync completed![/]";
    }
}