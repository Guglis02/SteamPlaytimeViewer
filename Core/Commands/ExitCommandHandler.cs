namespace SteamPlaytimeViewer.Core.Commands;

public class ExitCommandHandler : ICommandHandler
{
    public string Description => "Exit the application";

    public Task<bool> HandleAsync(string[] args, AppState state)
    {
        state.ShouldExit = true;
        return Task.FromResult(true);
    }
}