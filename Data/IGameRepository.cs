using SteamPlaytimeViewer.Core;
using SteamPlaytimeViewer.Data.Dtos;

public interface IGameRepository
{
    Task<List<Game>> GetGamesByUserAsync(string username);
    Task<bool> UserExistsAsync(string username);
    Task SaveUserAsync(string steamId, string nickname);
    Task SaveGamesAsync(string steamId, List<GameImportDto> gamesRawData);
}