using Gameloop.Vdf;
using SteamPlaytimeViewer.Core;
using SteamPlaytimeViewer.Data.Dtos;
using SteamPlaytimeViewer.External.SteamApi;
using SteamPlaytimeViewer.Models;

namespace SteamPlaytimeViewer.Services;

public class LocalVdfService
{
    private readonly IGameRepository _repository;
    private readonly SteamApiConnection _steamApi;
    private readonly AppState state;

    public LocalVdfService(SteamApiConnection steamApi, IGameRepository repository, AppState appState)
    {
        _steamApi = steamApi;
        _repository = repository;
        state = appState;
    }

    public bool UserFolderExists(UserInfo userInfo, string steamPath)
    {        
        if (steamPath == null)
        {
            state.StatusMessage = $"[red]Steam folder not found: {steamPath}[/]";
            return false;
        }

        string id3 = SteamId64ToSteamId3(userInfo.SteamId);
        string filePath = Path.Combine(steamPath, "userdata", id3, "config", "localconfig.vdf");

        if (!File.Exists(filePath))
        {
            state.StatusMessage = $"[red]VDF not found in: {filePath}[/]";
            return false;
        }

        return true;
    }


    public async Task<bool> SyncLocalLibraryAsync(UserInfo userInfo, string steamPath)
    {                
        if (!UserFolderExists(userInfo, steamPath))
        {
            return false;
        }
     
        string steamId = userInfo.SteamId;
        string id3 = SteamId64ToSteamId3(steamId);
        string filePath = Path.Combine(steamPath, "userdata", id3, "config", "localconfig.vdf");
        List<GameImportDto> localGamesCandidates = GetLocalLibraryCandidates(steamId, filePath);
                
        state.StatusMessage = $"[cyan]Found {localGamesCandidates.Count} games in VDF. Enriching data from API... (This can take a while)[/]";

        var gamesToSave = new List<GameImportDto>();
        int count = 0;

        foreach (var candidate in localGamesCandidates)
        {
            count++;
            state.StatusMessage = $"[cyan]Enriching {count}/{localGamesCandidates.Count}: AppID {candidate.AppId}...[/]";

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
                    state.StatusMessage = $"[yellow]Searching store for name...[/]";
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
                state.StatusMessage = $"[yellow]Error processing {candidate.AppId}: {ex.Message}[/]";
                return false;
            }
        }

        state.StatusMessage = "[cyan]Saving VDF data to database...[/]";
        await _repository.SaveGamesAsync(steamId, gamesToSave);
        state.StatusMessage = "[green]Local sync completed![/]";
        return true;
    }

    private List<GameImportDto> GetLocalLibraryCandidates(string steamId64, string filePath)
    {        
        var parseSettings = new VdfSerializerSettings {
            MaximumTokenSize = 20000,
            UsesEscapeSequences = true,
            UsesConditionals = false
        };
                    
        try 
        {
            state.StatusMessage = $"[cyan]Reading VDF: {filePath}[/]";

            dynamic vdf = VdfConvert.Deserialize(File.ReadAllText(filePath), parseSettings);
            var apps = vdf.Value.Software.Valve.Steam.apps;
            var results = new List<GameImportDto>();

            foreach (var app in apps)
            {
                string appIdStr = app.Key;
                string? playtimeStr = app.Value.Playtime?.ToString();
                string? lastPlayedStr = app.Value.LastPlayed?.ToString();

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
            state.StatusMessage = $"[red]Error reading VDF: {ex.Message}[/]";
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