using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using SteamPlaytimeViewer.Data.Dtos;
using SteamPlaytimeViewer.External.SteamApi;

namespace SteamPlaytimeViewer.Services;

public class LocalVdfService
{
    private readonly IGameRepository _repository;
    private readonly SteamApiConnection _steamApi;
    
    // Por enquanto fica esse placeholder
    private readonly string steamPath = @"C:\Program Files (x86)\Steam"; 

    public LocalVdfService(SteamApiConnection steamApi, IGameRepository repository)
    {
        _steamApi = steamApi;
        _repository = repository;
    }

    public async Task SyncLocalLibraryAsync(string steamId)
    {
        Console.WriteLine("Lendo arquivo VDF local...");
        
        List<GameImportDto> localGamesCandidates = GetLocalLibraryCandidates(steamId, steamPath);
        
        Console.WriteLine($"Encontrados {localGamesCandidates.Count} jogos no VDF local.");
        Console.WriteLine("Buscando nomes e achievements na API (Isso pode demorar)...");

        var gamesToSave = new List<GameImportDto>();
        int count = 0;

        foreach (var candidate in localGamesCandidates)
        {
            count++;
            Console.Write($"\rEnriquecendo VDF {count}/{localGamesCandidates.Count}: ID {candidate.AppId}...");

            try
            {
                var stats = await _steamApi.GetGameStatsAsync(steamId, candidate.AppId);

                string finalName = "Unknown";

                if (stats.GameName != "Unknown" && stats.GameName != "Error")
                {
                    finalName = stats.GameName;
                }
                else
                {
                    Console.Write(" (Buscando nome na loja)...");
                    finalName = await _steamApi.GetGameNameAsync(candidate.AppId);
                    
                    if (finalName == "Unknown")
                    {
                        finalName = $"App {candidate.AppId} (Local)";
                    }
                }

                var enrichedGame = new GameImportDto(
                    candidate.AppId,
                    finalName,
                    stats.TotalAchievements,
                    stats.UnlockedAchievements,
                    candidate.PlaytimeHours,
                    stats.FirstUnlockedTime,
                    candidate.LastPlayed
                );

                gamesToSave.Add(enrichedGame);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar {candidate.AppId}: {ex.Message}");
            }
        }

        Console.WriteLine("\nSalvando dados do VDF no banco...");
        await _repository.SaveGamesAsync(steamId, gamesToSave);
        Console.WriteLine("Sincronização Local Concluída!");
    }

    private List<GameImportDto> GetLocalLibraryCandidates(string steamId64, string steamPath)
    {        
        string id3 = SteamId64ToSteamId3(steamId64);
        string filePath = Path.Combine(steamPath, "userdata", id3, "config", "localconfig.vdf");

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"[Erro] VDF não encontrado em: {filePath}");
            return new List<GameImportDto>();
        }

        var parseSettings = new VdfSerializerSettings {
            MaximumTokenSize = 20000,
            UsesEscapeSequences = true,
            UsesConditionals = false
        };
                    
        try 
        {
            Console.WriteLine($"Lendo VDF: {filePath}");

            dynamic vdf = VdfConvert.Deserialize(File.ReadAllText(filePath), parseSettings);
            var apps = vdf.Value.Software.Valve.Steam.apps;
            var results = new List<GameImportDto>();

            foreach (var app in apps)
            {
                string appIdStr = app.Key;
                string playtimeStr = app.Value.Playtime?.ToString();
                string lastPlayedStr = app.Value.LastPlayed?.ToString();

                if (int.TryParse(appIdStr, out int appId) && 
                    int.TryParse(playtimeStr, out int minutes) && 
                    minutes > 0)
                {
                    DateTime? lastPlayedDate = null;
                    if (long.TryParse(lastPlayedStr, out long lastPlayedUnix) && lastPlayedUnix > 0)
                    {
                        lastPlayedDate = DateTimeOffset.FromUnixTimeSeconds(lastPlayedUnix).DateTime;
                    }

                    results.Add(new GameImportDto(
                        appId,
                        "Unknown", // Será preenchido pela API
                        0, 0, 
                        minutes / 60.0,
                        null, 
                        lastPlayedDate
                    ));
                }
            }
            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao ler VDF: {ex.Message}");
            return new List<GameImportDto>();
        }
    }

    private static string SteamId64ToSteamId3(string steamId)
    {
        long steamId64 = long.Parse(steamId);
        long steamId3 = steamId64 - 76561197960265728;
        return steamId3.ToString();
    }
}