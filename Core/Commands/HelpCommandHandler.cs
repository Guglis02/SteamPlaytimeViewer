using System.Text;

namespace SteamPlaytimeViewer.Core.Commands;

public class HelpCommandHandler : ICommandHandler
{
    private readonly CommandRegistry _registry;

    public HelpCommandHandler(CommandRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public string Description => "Mostra a lista de comandos disponiveis ou detalhes sobre um comando especifico";

    public Task<bool> HandleAsync(string[] args, AppState state)
    {
        var commands = _registry.GetAllCommands();
        
        if (args.Length == 0)
        {
            var sb = new StringBuilder();
            sb.Append("[bold]Comandos disponíveis ('help <command>' para mais detalhes):[/]");
            
            foreach (var cmd in commands.OrderBy(x => x.Key))
            {
                sb.Append($" {cmd.Key};");
            }

            state.StatusMessage = sb.ToString();
            return Task.FromResult(true);
        }

        if (args.Length > 1)
        {
            state.StatusMessage = "[red]O comando help aceita apenas um comando como parâmetro![/]";
            return Task.FromResult(false);
        }

        if (!commands.ContainsKey(args[0]))
        {
            state.StatusMessage = $"[yellow]O comando {args[0]} não foi encontrado! Use o comando 'help' para ver a lista de comandos disponíveis.[/]";
            return Task.FromResult(false);
        }

        state.StatusMessage = $"[bold]{args[0].ToLower()}:[/] {commands[args[0]]}";
        return Task.FromResult(true);
    }
}