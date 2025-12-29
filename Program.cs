using System.Text;
using Spectre.Console;
using SteamPlaytimeViewer.Data;
using SteamPlaytimeViewer;
using SteamPlaytimeViewer.Core;
using SteamPlaytimeViewer.Services;
using Microsoft.Extensions.Configuration;
using SteamPlaytimeViewer.External.SteamAPI;
using SteamPlaytimeViewer.External.SteamApi;
using SteamPlaytimeViewer.Core.Commands;

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

        // Local VDF Setup
        LocalVdfService localVdfService = new LocalVdfService(steamApiConnection, repository);
        // ------

        // Data service Setup
        var dataService = new DataService(steamApiConnection, repository);
        // ------

        // Command Setup
        var commandRegistry = new CommandRegistry();
        commandRegistry.Register("exit", new ExitCommandHandler());
        commandRegistry.Register("help", new HelpCommandHandler(commandRegistry));
        commandRegistry.Register("user", new UserCommandHandler(dataService));
        commandRegistry.Register("sort", new SortCommandHandler());
        commandRegistry.Register("search", new SearchCommandHandler());
        commandRegistry.Register("sync", new SyncCommandHandler(steamSyncService, localVdfService));
        commandRegistry.Register("folder", new FolderCommandHandler());

        var inputHandler = new InputHandler(commandRegistry);
        // ------

        // AppState Setup
        string initialMessage = "Type 'help' for a list of commands and 'help <command>' for details.";
        var state = new AppState()
        {
            StatusMessage = initialMessage,
            CurrentUser = new UserInfo (username: "hyan", steamId: "76561198062983485"),
            TerminalHeight = Console.WindowHeight,
            TerminalWidth = Console.WindowWidth,
            SteamFolder = SteamPathFinder.TryAutoDetectSteamPath() ?? ""
        };
        state.AllGames = await dataService.GetGamesAsync(state.CurrentUser.Username);
        // ------

        Console.OutputEncoding = Encoding.UTF8;

        while (true)
        {
            // Handle terminal resize
            if (state.TerminalHeight != Console.WindowHeight ||
                state.TerminalWidth != Console.WindowWidth)
            {
                state.TerminalHeight = Console.WindowHeight;
                state.TerminalWidth = Console.WindowWidth;
                if (state.TerminalHeight > state.TerminalMinSize)
                    state.MarkDirty();
            }

            if (state.ShouldUpdateList)
            {
                state.AllGames = await dataService.GetGamesAsync(state.CurrentUser.Username,
                                                                 state.SearchQuery,
                                                                 state.SortColumn,
                                                                 state.SortAscending);
                state.ShouldUpdateList = false;
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
            }

            if (state.ShouldExit)
            {
                break;
            }

            await Task.Delay(50);
        }
    }
}
