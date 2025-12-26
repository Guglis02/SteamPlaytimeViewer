using SteamPlaytimeViewer.Core;
using SteamPlaytimeViewer.Data;

public class SqliteGameRepository : IGameRepository
{
    private SteamDbContext dbContext;

    public SqliteGameRepository(SteamDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<List<Game>> GetGamesByUserAsync(string username)
    {
        throw new NotImplementedException();
    }

    public Task SaveGamesAsync(string username, List<Game> games)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UserExistsAsync(string username)
    {
        throw new NotImplementedException();
    }
}