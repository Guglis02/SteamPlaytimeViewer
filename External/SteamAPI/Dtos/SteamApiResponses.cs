using System.Text.Json.Serialization;

namespace SteamPlaytimeViewer.External.SteamApi.Dtos;

// --- DTOs Genéricos de Resposta ---
public record SteamResponse<T>(
    [property: JsonPropertyName("response")] T Data
);

public record SteamStatsResponse(
    [property: JsonPropertyName("playerstats")] PlayerStatsData Data
);

// --- Dados de Jogos (GetOwnedGames) ---
public record OwnedGamesData(
    [property: JsonPropertyName("game_count")] int GameCount,
    [property: JsonPropertyName("games")] List<SteamGameDto> Games
);

public record SteamGameDto(
    [property: JsonPropertyName("appid")] int AppId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("playtime_forever")] int PlaytimeForeverMinutes, // Em minutos
    [property: JsonPropertyName("rtime_last_played")] long LastPlayedUnix     // Unix Timestamp
);

// --- Dados de Usuário (GetPlayerSummaries) ---
public record PlayerSummaryData(
    [property: JsonPropertyName("players")] List<SteamPlayerDto> Players
);

public record SteamPlayerDto(
    [property: JsonPropertyName("steamid")] string SteamId,
    [property: JsonPropertyName("personaname")] string Nickname,
    [property: JsonPropertyName("avatarfull")] string AvatarUrl
);

// --- Dados de Achievements (GetPlayerAchievements) ---
public record PlayerStatsData(
    [property: JsonPropertyName("gameName")] string GameName,
    [property: JsonPropertyName("achievements")] List<SteamAchievementDto> Achievements,
    [property: JsonPropertyName("success")] bool Success
);

public record SteamAchievementDto(
    [property: JsonPropertyName("apiname")] string ApiName,
    [property: JsonPropertyName("achieved")] int Achieved, // 0 ou 1
    [property: JsonPropertyName("unlocktime")] long UnlockTimeUnix
);

// --- DTO Simplificado para devolver pro seu Service ---
public record GameStatsResult(
    int TotalAchievements,
    int UnlockedAchievements,
    DateTime? FirstUnlockedTime
);