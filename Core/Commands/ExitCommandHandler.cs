namespace SteamPlaytimeViewer.Core.Commands;

public class ExitCommandHandler : ICommandHandler
{
    public string Description => "Sai da aplicação";

    public Task<bool> HandleAsync(string[] args, AppState state)
    {
        state.ShouldExit = true;
        return Task.FromResult(true);
    }
}