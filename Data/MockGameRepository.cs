using SteamPlaytimeViewer.Core;
using SteamPlaytimeViewer.Data.Dtos;

public class MockGameRepository : IGameRepository
{
    private Dictionary<string, List<Game>> _userGames;

    public MockGameRepository()
    {
        _userGames = new();

        var guglis = new List<Game>();
        for (int i = 0; i < 50; i += 2)
        {
            guglis.Add(new Game(
                $"Elden Ring {i}", $"{120+i}h", "42/42", "100%", "2022-02-25", "2024-01-01"
                ));
            guglis.Add(new Game(
                "Hollow Knight", "50h", "63/63", "100%", "2021-05-10", "2023-11-15" 
                ));
        }

        var fulano = new List<Game>
        {
            new Game("Dota 2", "3000h", "0/0", "0%", "2015-01-01", "Hoje"),
            new Game("CS:GO", "500h", "10/167", "5%", "2016-02-20", "Ontem")
        };

        var teste = new List<Game> { };
        
        _userGames.Add("Guglis", guglis);
        _userGames.Add("Fulano", fulano);
        _userGames.Add("Teste", teste);
    }

    public Task<List<Game>> GetGamesByUserAsync(string username)
    {
        _userGames.TryGetValue(username, out var games);
        return Task.FromResult(games ?? new List<Game>());
    }

    public Task<bool> UserExistsAsync(string username)
    {
        return Task.FromResult(_userGames.ContainsKey(username));
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