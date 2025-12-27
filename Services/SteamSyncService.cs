using SteamPlaytimeViewer.Data.Dtos;
using SteamPlaytimeViewer.External.SteamApi;

public class SteamSyncService
{
    private readonly SteamApiConnection _steamApi;
    private readonly IGameRepository _repository;

    public SteamSyncService(SteamApiConnection steamApi, IGameRepository repository)
    {
        _steamApi = steamApi;
        _repository = repository;
    }

    public async Task SyncUserDataAsync(string steamId)
    {
        Console.WriteLine($"Iniciando sincronização para: {steamId}...");

        var playerSummary = await _steamApi.GetPlayerSummaryAsync(steamId); 
        await _repository.SaveUserAsync(steamId, playerSummary.Nickname);

        var ownedGames = await _steamApi.GetOwnedGamesAsync(steamId);

        var playedGames = ownedGames
            .Where(g => g.PlaytimeForeverMinutes > 0) 
            .ToList();

        Console.WriteLine($"Encontrados {playedGames.Count} jogos jogados. Buscando detalhes...");

        var gamesToSave = new List<GameImportDto>();
        int count = 0;

        foreach (var game in playedGames)
        {
            count++;
            Console.Write($"\rProcessando {count}/{playedGames.Count}: {game.Name}...");

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
                Console.WriteLine($"\n[Aviso] Sem stats para {game.Name}. Salvando básico.");
                
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

        Console.WriteLine("\nSalvando no banco de dados...");
        await _repository.SaveGamesAsync(steamId, gamesToSave);
        
        Console.WriteLine("Sincronização concluída!");
    }
}