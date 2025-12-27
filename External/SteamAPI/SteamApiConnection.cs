using System.Net;
using System.Text.Json;
using SteamPlaytimeViewer.External.SteamApi.Dtos;

namespace SteamPlaytimeViewer.External.SteamApi;

public class SteamApiConnection
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    
    // Intervalo de segurança para evitar HTTP 429 (Too Many Requests)
    private const int RequestDelayMs = 800; 

    public SteamApiConnection(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
    }

    public async Task<SteamPlayerDto> GetPlayerSummaryAsync(string steamId)
    {
        await Task.Delay(RequestDelayMs);
        var url = $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={_apiKey}&steamids={steamId}";
        
        var response = await _httpClient.GetStringAsync(url);
        var data = JsonSerializer.Deserialize<SteamResponse<PlayerSummaryData>>(response);

        return data?.Data.Players.FirstOrDefault() 
               ?? new SteamPlayerDto(steamId, "Unknown", "");
    }

    public async Task<List<SteamGameDto>> GetOwnedGamesAsync(string steamId)
    {
        await Task.Delay(RequestDelayMs);

        var url = $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={_apiKey}&steamid={steamId}&format=json&include_appinfo=1&include_played_free_games=1";

        var response = await _httpClient.GetStringAsync(url);
        var data = JsonSerializer.Deserialize<SteamResponse<OwnedGamesData>>(response);

        return data?.Data.Games ?? new List<SteamGameDto>();
    }

    public async Task<GameStatsResult> GetGameStatsAsync(string steamId, int appId)
    {
        await Task.Delay(RequestDelayMs);

        var url = $"http://api.steampowered.com/ISteamUserStats/GetPlayerAchievements/v0001/?key={_apiKey}&steamid={steamId}&appid={appId}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            // A API retorna 400 Bad Request se o jogo não tiver achievements 
            if (!response.IsSuccessStatusCode)
            {
                return new GameStatsResult(0, 0, null);
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<SteamStatsResponse>(json);

            var achievements = data?.Data?.Achievements;

            if (achievements == null || !achievements.Any())
            {
                return new GameStatsResult(0, 0, null);
            }

            var total = achievements.Count;
            var unlockedList = achievements.Where(a => a.Achieved == 1).ToList();
            var unlockedCount = unlockedList.Count;

            DateTime? firstUnlock = null;
            if (unlockedList.Any())
            {
                // Pega o menor timestamp (exceto 0)
                var minUnix = unlockedList
                    .Select(a => a.UnlockTimeUnix)
                    .Where(t => t > 0)
                    .DefaultIfEmpty(0)
                    .Min();

                if (minUnix > 0)
                {
                    firstUnlock = DateTimeOffset.FromUnixTimeSeconds(minUnix).DateTime;
                }
            }

            return new GameStatsResult(total, unlockedCount, firstUnlock);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"[API Error] Falha ao buscar stats do AppId {appId}: {ex.Message}");
            return new GameStatsResult(0, 0, null);
        }
    }
}