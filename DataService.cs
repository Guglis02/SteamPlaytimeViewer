namespace SteamPlaytimeViewer;

public class DataService
{
    private Dictionary<string, List<Game>> gameDatabase;

    public DataService()
    {
        gameDatabase = new();
        InitializeMockData();
    }

    public List<Game> GetGames(string username)
    {
        if (gameDatabase.ContainsKey(username))
            return gameDatabase[username];
        
        return new List<Game>();
    }

    public bool HaveUser(string username)
    {
        return gameDatabase.ContainsKey(username);
    }

    private void InitializeMockData()
    {
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
        
        gameDatabase.Add("Guglis", guglis);
        gameDatabase.Add("Fulano", fulano);
        gameDatabase.Add("Teste", teste);
    }
}