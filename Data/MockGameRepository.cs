using SteamPlaytimeViewer.Core;
using SteamPlaytimeViewer.Data.Dtos;

public class MockGameRepository : IGameRepository
{
    private Dictionary<string, List<GameView>> _userGames;

    public MockGameRepository()
    {
        _userGames = new();

        var guglis = new List<GameView>();
        for (int i = 0; i < 50; i += 2)
        {
            guglis.Add(new GameView(
                $"Elden Ring {i}", $"{120 + i}h", "42/42", "100%", "2022-02-25", "2024-01-01"
                ));
            guglis.Add(new GameView(
                "Hollow Knight", "50h", "63/63", "100%", "2021-05-10", "2023-11-15"
                ));
        }

        var fulano = new List<GameView>
        {
            new GameView("Dota 2", "3000h", "0/0", "0%", "2015-01-01", "Hoje"),
            new GameView("CS:GO", "500h", "10/167", "5%", "2016-02-20", "Ontem")
        };

        var teste = new List<GameView> { };

        _userGames.Add("Guglis", guglis);
        _userGames.Add("Fulano", fulano);
        _userGames.Add("Teste", teste);
    }

    public Task<List<GameView>> GetGamesByUserAsync(string username,
                                                    string? searchFilter = null,
                                                    string sortColumn = "Title",
                                                    bool sortAscending = true)
    {
        // Por enquanto o mock vai ignorar filtro e sorting
        _userGames.TryGetValue(username, out var games);
        return Task.FromResult(games ?? new List<GameView>());
    }

    public Task<bool> UserExistsAsync(string username)
    {
        return Task.FromResult(_userGames.ContainsKey(username));
    }

    public async Task<bool> UserExistsBySteamIdAsync(string steamId)
    {
        return await Task.FromResult(false);
    }

    public async Task<string?> GetUserNicknameBySteamIdAsync(string steamId)
    {
        return await Task.FromResult<string?>(null);
    }

    public async Task<string?> GetSteamIdByUsernameAsync(string username)
    {
        return await Task.FromResult<string?>(null);
    }

    public Task SaveUserAsync(string steamId, string username)
    {
        throw new NotImplementedException();
    }

    public Task SaveGamesAsync(string username, List<GameImportDto> games)
    {
        return Task.CompletedTask;
    }
}