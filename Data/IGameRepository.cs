using SteamPlaytimeViewer.Core;
using SteamPlaytimeViewer.Data.Dtos;

public interface IGameRepository
{
    Task<List<GameView>> GetGamesByUserAsync(string username,
                                             string? searchFilter,
                                             string sortColumn,
                                             bool sortAscending);
    Task<bool> UserExistsAsync(string username);
    Task<bool> UserExistsBySteamIdAsync(string steamId);

    Task<string?> GetUserNicknameBySteamIdAsync(string steamId);
    Task<string?> GetSteamIdByUsernameAsync(string username);

    Task SaveUserAsync(string steamId, string nickname);
    Task SaveGamesAsync(string steamId, List<GameImportDto> gamesRawData);
}