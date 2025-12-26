using System.Text;
using Spectre.Console;
using SteamPlaytimeViewer.Data;
using SteamPlaytimeViewer;
using SteamPlaytimeViewer.Core;
using SteamPlaytimeViewer.Services;

public class Program
{
    static readonly bool useRealDb = false;
    
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        SteamDbContext dbContext = new SteamDbContext();

        IGameRepository repository = useRealDb 
            ? new SqliteGameRepository(dbContext)
            : new MockGameRepository();

        var dataService = new DataService(repository);
        
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
