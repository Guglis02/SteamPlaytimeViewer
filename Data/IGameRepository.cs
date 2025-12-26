using SteamPlaytimeViewer.Core;

public interface IGameRepository
{
    Task<List<Game>> GetGamesByUserAsync(string username);
    Task<bool> UserExistsAsync(string username);
    Task SaveGamesAsync(string username, List<Game> games);
}