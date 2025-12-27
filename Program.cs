using System.Text;
using Spectre.Console;
using SteamPlaytimeViewer.Data;
using SteamPlaytimeViewer;
using SteamPlaytimeViewer.Core;
using SteamPlaytimeViewer.Services;
using Microsoft.Extensions.Configuration;
using SteamPlaytimeViewer.External.SteamAPI;
using SteamPlaytimeViewer.External.SteamApi;

public class Program
{
    static readonly bool useRealDb = true;
    
    public static async Task Main(string[] args)
    {
        // Database setup
        SteamDbContext dbContext = new SteamDbContext();

        IGameRepository repository = useRealDb 
            ? new SqliteGameRepository(dbContext)
            : new MockGameRepository();

        var dataService = new DataService(repository);
        // ------

        // API Setup
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Secret.json", optional: true, reloadOnChange: true);
            
            IConfiguration config = builder.Build();

        var steamSettings = config.GetSection("SteamSettings").Get<SteamApiSettings>();

        if (string.IsNullOrEmpty(steamSettings?.ApiKey) || steamSettings.ApiKey == "YOUR_KEY_HERE")
        {
            Console.WriteLine("Erro: API Key não configurada.");
            return;
        }

        HttpClient httpClient = new();
        SteamApiConnection steamApiConnection = new SteamApiConnection(httpClient, steamSettings.ApiKey);
        SteamSyncService steamSyncService = new SteamSyncService(steamApiConnection, repository);
        // ------

        Console.OutputEncoding = Encoding.UTF8;
        
        string helpMessage = "Type 'user <name>' to change profile or 'exit'.";

        var state = new AppState(helpMessage);
        state.TerminalHeight = Console.WindowHeight;
        
        state.AllGames = await dataService.GetGamesAsync(state.CurrentUser);

        var inputHandler = new InputHandler(dataService, helpMessage);

        while (true)
        {
            // Handle terminal resize
            if (state.TerminalHeight != Console.WindowHeight)
            {
                state.TerminalHeight = Console.WindowHeight;
                if (state.TerminalHeight > state.TerminalMinSize)
                    state.MarkDirty();
            }

            // Render UI
            if (state.IsDirty)
            {
                AnsiConsole.Clear();
                var layout = UserInterface.BuildLayout(
                    state.CurrentUser,
                    state.VisibleGames,
                    state.AllGames.Count,
                    state.ScrollIndex,
                    state.InputBuffer.ToString(),
                    state.StatusMessage);
                AnsiConsole.Write(layout);
                state.ClearDirty();
            }

            // Handle input
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);
                await inputHandler.ProcessInputAsync(key, state);

                if (inputHandler.IsExitCommand(state.InputBuffer.ToString()))
                {
                    break;
                }
            }

            await Task.Delay(50);
        }
    }
}
