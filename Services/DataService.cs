using SteamPlaytimeViewer.Core;

namespace SteamPlaytimeViewer.Services;

public class DataService
{
    private readonly IGameRepository _repository;

    public DataService(IGameRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<GameView>> GetGamesAsync(string username, 
                                             string? searchFilter = null,
                                             string sortColumn = nameof(GameView.Title),
                                             bool sortAscending = true)
    {
        return await _repository.GetGamesByUserAsync(username, searchFilter, sortColumn, sortAscending);
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        return await _repository.UserExistsAsync(username);
    }

}