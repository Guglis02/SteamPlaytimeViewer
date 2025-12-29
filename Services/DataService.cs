using SteamPlaytimeViewer.Core;
using SteamPlaytimeViewer.External.SteamApi;

namespace SteamPlaytimeViewer.Services;

public class DataService
{
    private readonly IGameRepository _repository;
    private readonly SteamApiConnection? _steamApiConnection;

    public DataService(SteamApiConnection steamApiConnection, IGameRepository repository)
    {
        _repository = repository;
        _steamApiConnection = steamApiConnection;
    }

    public async Task<List<GameView>> GetGamesAsync(string? username, 
                                             string? searchFilter = null,
                                             string sortColumn = nameof(GameView.Title),
                                             bool sortAscending = true)
    {
        if (username == null)
        {
            return new List<GameView>();
        }

        return await _repository.GetGamesByUserAsync(username, searchFilter, sortColumn, sortAscending);
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        return await _repository.UserExistsAsync(username);
    }

    public async Task<bool> UserExistsBySteamIdAsync(string steamId)
    {
        return await _repository.UserExistsBySteamIdAsync(steamId);
    }

    public async Task<string?> GetUserNicknameBySteamIdAsync(string steamId)
    {
        return await _repository.GetUserNicknameBySteamIdAsync(steamId);
    }

    public async Task<UserInfo?> ResolveBySteamIdAsync(string steamId)
    {
        var existingNickname = await GetUserNicknameBySteamIdAsync(steamId);
        if (existingNickname != null)
        {
            return new UserInfo (steamId: steamId, username: existingNickname);
        }

        if (_steamApiConnection == null)
        {
            throw new InvalidOperationException("Steam API não configurada. Não é possível buscar usuário desconhecido.");
        }

        try
        {
            var playerSummary = await _steamApiConnection.GetPlayerSummaryAsync(steamId);
            
            if (playerSummary == null || string.IsNullOrWhiteSpace(playerSummary.Nickname))
            {
                return null;
            }

            await _repository.SaveUserAsync(steamId, playerSummary.Nickname);

            return new UserInfo (steamId: steamId, username: playerSummary.Nickname);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao buscar usuário na Steam API: {ex.Message}", ex);
        }
    }

    public async Task<string?> GetSteamIdByUsernameAsync(string username)
    {
        return await _repository.GetSteamIdByUsernameAsync(username);
    }
}