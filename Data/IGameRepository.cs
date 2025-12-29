using SteamPlaytimeViewer.Core;
using SteamPlaytimeViewer.Data.Dtos;

public interface IGameRepository
{
    Task<List<GameView>> GetGamesByUserAsync(string username,
                                             string? searchFilter,
                                             string sortColumn,
                                             bool sortAscending);
    Task<bool> UserExistsAsync(string username);
    Task SaveUserAsync(string steamId, string nickname);
    Task SaveGamesAsync(string steamId, List<GameImportDto> gamesRawData);
}