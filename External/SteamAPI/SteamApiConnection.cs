using System.Text.Json;
using System.Text.Json.Nodes;
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

            // A api retorna erro se o jogo não tem stats ou perfil privado
            if (!response.IsSuccessStatusCode)
            {
                return new GameStatsResult("Unknown", 0, 0, null);
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<SteamStatsResponse>(json);

            string gameName = data?.Data?.GameName ?? "Unknown";

            var achievements = data?.Data?.Achievements;

            if (achievements == null || !achievements.Any())
            {
                return new GameStatsResult(gameName, 0, 0, null);
            }

            var total = achievements.Count;
            var unlockedList = achievements.Where(a => a.Achieved == 1).ToList();
            var unlockedCount = unlockedList.Count;

            DateTime? firstUnlock = null;
            if (unlockedList.Any())
            {
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

            return new GameStatsResult(gameName, total, unlockedCount, firstUnlock);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API Error] AppId {appId}: {ex.Message}");
            return new GameStatsResult("Error", 0, 0, null);
        }
    }

    public async Task<string> GetGameNameAsync(int appId)
    {
        await Task.Delay(RequestDelayMs); 

        var url = $"http://store.steampowered.com/api/appdetails?appids={appId}&filters=basic";

        try
        {
            var response = await _httpClient.GetStringAsync(url);
            
            var node = JsonNode.Parse(response);
            
            var appNode = node?[appId.ToString()];
            
            if (appNode?["success"]?.GetValue<bool>() == true)
            {
                return appNode?["data"]?["name"]?.GetValue<string>() ?? "Unknown";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[API Error] AppId {appId}: {ex.Message}");
            return "Unknown";
        }

        return "Unknown";
    }

    public async Task<string> ParseSteamIdAsync(string input)
    {
        input = input.Trim();

        // O usuário colou um ID numérico direto (ex: "76561198...")
        if (long.TryParse(input, out _))
            return input;

        // O usuário colou uma URL do tipo /profiles/
        if (input.Contains("/profiles/"))
        {
            var parts = input.Split(new[] { "/profiles/" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
            {
                return parts[1].Split('/')[0];
            }
        }

        // O usuário colou uma URL Customizada (/id/)
        string vanityName = input;
        
        if (input.Contains("/id/"))
        {
            var parts = input.Split(new[] { "/id/" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
            {
                vanityName = parts[1].Split('/')[0];
            }
        }

        await Task.Delay(RequestDelayMs);
        var url = $"http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={_apiKey}&vanityurl={vanityName}";

        try 
        {
            var response = await _httpClient.GetStringAsync(url);
            var data = JsonSerializer.Deserialize<SteamResponse<ResolveVanityResponse>>(response);

            if (data?.Data?.Success == 1)
            {
                return data.Data.SteamId;
            }
        }
        catch 
        {
        }

        return string.Empty;
    }
}