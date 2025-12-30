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

    public async Task<List<GameView>> GetGamesAsync(string username, 
                                             string? searchFilter = null,
                                             string sortColumn = nameof(GameView.Title),
                                             bool sortAscending = true)
    {
        if (string.IsNullOrWhiteSpace(username))
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

    public async Task<string> ResolveVanityUrlAsync(string vanityName)
    {
        if (_steamApiConnection == null)
        {
            throw new InvalidOperationException("Steam API não configurada.");
        }

        return await _steamApiConnection.ResolveVanityUrlAsync(vanityName);
    }

    public async Task<UserInfo?> ResolveBySteamIdAsync(string steamId)
    {
        var existingNickname = await GetUserNicknameBySteamIdAsync(steamId);
        if (existingNickname != null)
        {
            return new UserInfo(steamId, existingNickname);
        }

        if (_steamApiConnection == null)
            return null;

        try
        {
            var playerSummary = await _steamApiConnection.GetPlayerSummaryAsync(steamId);
            
            if (playerSummary == null || string.IsNullOrWhiteSpace(playerSummary.Nickname))
                return null;

            // NUNCA salva - isso é feito explicitamente em outros comandos
            return new UserInfo(steamId, playerSummary.Nickname);
        }
        catch
        {
            return null;
        }
    }

    public async Task<UserInfo?> ResolveByUsernameAsync(string username)
    {
        string? steamId = await _repository.GetSteamIdByUsernameAsync(username);
        
        if (steamId == null)
            return null;
        
        string? nickname = await _repository.GetUserNicknameBySteamIdAsync(steamId);
        
        return new UserInfo(steamId, nickname ?? username);
    }
}