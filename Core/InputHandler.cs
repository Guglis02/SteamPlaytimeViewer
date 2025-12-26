using SteamPlaytimeViewer.Services;

namespace SteamPlaytimeViewer.Core;

public class InputHandler
{
    private readonly DataService _dataService;
    private readonly string _helpMessage;

    public InputHandler(DataService dataService, string helpMessage)
    {
        _dataService = dataService;
        _helpMessage = helpMessage;
    }

    public async Task<bool> ProcessInputAsync(ConsoleKeyInfo key, AppState state)
    {
        state.MarkDirty();

        if (key.Key == ConsoleKey.UpArrow)
        {
            if (state.ScrollIndex > 0) state.ScrollIndex--;
            return true;
        }
        else if (key.Key == ConsoleKey.DownArrow)
        {
            int maxScroll = Math.Max(0, state.AllGames.Count - state.ItemsPerPage);
            if (state.ScrollIndex < maxScroll) state.ScrollIndex++;
            return true;
        }
        else if (key.Key == ConsoleKey.Backspace)
        {
            if (state.InputBuffer.Length > 0)
                state.InputBuffer.Length--;
            return true;
        }
        else if (key.Key == ConsoleKey.Enter)
        {
            await ProcessCommandAsync(state.InputBuffer.ToString().Trim(), state);
            state.InputBuffer.Clear();
            return true;
        }
        else if (!char.IsControl(key.KeyChar))
        {
            state.InputBuffer.Append(key.KeyChar);
            return true;
        }

        return false;
    }

    private async Task ProcessCommandAsync(string command, AppState state)
    {
        state.MarkDirty();
        
        if (command == "exit")
        {
            return;
        }
        else if (command == "help")
        {
            state.StatusMessage = _helpMessage;
        }
        else if (command.StartsWith("user "))
        {
            var newUser = command.Substring(5).Trim();
            if (await _dataService.UserExistsAsync(newUser))
            {
                state.CurrentUser = newUser;
                state.ScrollIndex = 0;
                state.AllGames = await _dataService.GetGamesAsync(newUser);
                state.StatusMessage = $"[green]User changed to {newUser}![/]";
            }
            else
            {
                state.StatusMessage = $"[red]User '{newUser}' not found![/]";
            }
        }
        else
        {
            state.StatusMessage = $"[yellow]Unknown command '{command}'![/]";
        }
    }

    public bool IsExitCommand(string input) => input.Trim() == "exit";
}