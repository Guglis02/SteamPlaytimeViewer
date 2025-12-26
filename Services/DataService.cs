using SteamPlaytimeViewer.Core;

namespace SteamPlaytimeViewer.Services;

public class DataService
{
    private readonly IGameRepository _repository;

    public DataService(IGameRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Game>> GetGamesAsync(string username)
    {
        return await _repository.GetGamesByUserAsync(username);
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        return await _repository.UserExistsAsync(username);
    }

}