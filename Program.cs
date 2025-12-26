using System.Text;
using Spectre.Console;
using SteamPlaytimeViewer;

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var dataService = new DataService();
        string helpMessage = "Type 'user <name>' to change profile or 'exit'.";

        var state = new AppState(helpMessage);
        state.TerminalHeight = Console.WindowHeight;
        state.AllGames = dataService.GetGames(state.CurrentUser);

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
                inputHandler.ProcessInput(key, state);

                if (inputHandler.IsExitCommand(state.InputBuffer.ToString()))
                {
                    break;
                }
            }

            Thread.Sleep(50);
        }
    }
}