using System.Text;

namespace SteamPlaytimeViewer.Core.Commands;

public class HelpCommandHandler : ICommandHandler
{
    private readonly CommandRegistry _registry;

    public HelpCommandHandler(CommandRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public string Description => "Shows a list of available commands or details about an specific command (Usage: help or help <command>)";

    public Task<bool> HandleAsync(string[] args, AppState state)
    {
        var commands = _registry.GetAllCommands();
        
        if (args.Length == 0)
        {
            var sb = new StringBuilder();
            sb.Append("[bold cyan]Available Commands:[/]");
            
            foreach (var cmd in commands.OrderBy(x => x.Key))
            {
                sb.Append($" {cmd.Key};");
            }

            state.StatusMessage = sb.ToString();
            return Task.FromResult(true);
        }

        if (args.Length > 1)
        {
            state.StatusMessage = "[yellow]Use: help for a list of commands or help <command> for more details[/]";
            return Task.FromResult(false);

        }

        if (!commands.ContainsKey(args[0]))
        {
            state.StatusMessage = $"[yellow]Command {args[0]} not found! Use 'help' for a list of available commands.[/]";
            return Task.FromResult(false);
        }

        state.StatusMessage = $"[bold]{args[0].ToLower()}:[/] {commands[args[0]]}";
        return Task.FromResult(true);
    }
}