using SteamPlaytimeViewer.Core;
using SteamPlaytimeViewer.Data;
using SteamPlaytimeViewer.Data.Dtos;
using Microsoft.EntityFrameworkCore;
using SteamPlaytimeViewer.Models;
using GameView = SteamPlaytimeViewer.Core.Game;

public class SqliteGameRepository : IGameRepository
{
    private SteamDbContext _dbContext;

    public SqliteGameRepository(SteamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<GameView>> GetGamesByUserAsync(string username)
    {        
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Nickname == username);

        if (user == null)
            return new List<GameView>();

        var rawGames = await _dbContext.UserGameStats
            .Where(ugs => ugs.UserSteamId == user.SteamId)
            .Include(ugs => ugs.Game)
            .ToListAsync();
        
        var games = rawGames.Select(ugs => new GameView(
                ugs.Game.Title,
                $"{ugs.PlaytimeHours.ToString("0.00")}h",
                $"{ugs.UnlockedAchievements}/{ugs.Game.TotalAchievements}",
                CalculatePercentage(ugs.UnlockedAchievements, ugs.Game.TotalAchievements).ToString() + "%",
                ugs.FirstPlayed?.ToString("yyyy-MM-dd") ?? "N/A",
                ugs.LastPlayed?.ToString("yyyy-MM-dd") ?? "N/A"
            )).ToList();

        return games ?? new List<GameView>();
    }

    public async Task SaveUserAsync(string steamId, string nickname)
    {
        var user = await _dbContext.Users.FindAsync(steamId);

        if (user == null)
        {
            user = new User 
            { 
                SteamId = steamId, 
                Nickname = nickname 
            };
            _dbContext.Users.Add(user);
        }
        else
        {
            if (user.Nickname != nickname)
            {
                user.Nickname = nickname;
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task SaveGamesAsync(string steamId, List<GameImportDto> gamesRawData)
    {
        var user = await _dbContext.Users.FindAsync(steamId);

        if (user == null)
        {
            Console.WriteLine($"[Error] User with ID {steamId} not found via SaveGamesAsync.");
            return; 
        }

        foreach (var importData in gamesRawData)
        {
            var dbGame = await _dbContext.Games.FindAsync(importData.AppId);
            
            if (dbGame == null)
            {
                dbGame = new SteamPlaytimeViewer.Models.Game
                {
                    AppId = importData.AppId,
                    Title = importData.Title,
                    TotalAchievements = importData.TotalAchievements
                };
                _dbContext.Games.Add(dbGame);
            }
            else
            {
                dbGame.TotalAchievements = importData.TotalAchievements;
            }

            var userStats = await _dbContext.UserGameStats
                .FindAsync(user.SteamId, importData.AppId);

            if (userStats == null)
            {
                userStats = new UserGameStats
                {
                    UserSteamId = user.SteamId,
                    GameAppId = importData.AppId,
                    PlaytimeHours = importData.PlaytimeHours,
                    UnlockedAchievements = importData.UnlockedAchievements,
                    FirstPlayed = importData.FirstPlayed,
                    LastPlayed = importData.LastPlayed
                };
                _dbContext.UserGameStats.Add(userStats);
            }
            else
            {
                userStats.PlaytimeHours = importData.PlaytimeHours;
                userStats.UnlockedAchievements = importData.UnlockedAchievements;
                userStats.LastPlayed = importData.LastPlayed;
                
                if (userStats.FirstPlayed == null && importData.FirstPlayed != null)
                {
                    userStats.FirstPlayed = importData.FirstPlayed;
                }
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.Nickname == username);
    }
    
    private static double CalculatePercentage(int unlocked, int total)
    {
        if (total == 0) return 0.0;
        return Math.Round((double)unlocked / total * 100, 1);
    }
}