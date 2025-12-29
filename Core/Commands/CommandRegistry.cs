namespace SteamPlaytimeViewer.Core.Commands;

/// <summary>
/// Registra e executa comandos da aplicação.
/// </summary>
public class CommandRegistry
{
    private readonly Dictionary<string, ICommandHandler> _commands = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registra um novo comando no sistema.
    /// </summary>
    /// <param name="name">Nome do comando (case-insensitive)</param>
    /// <param name="handler">Handler que processará o comando</param>
    public void Register(string name, ICommandHandler handler)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome do comando não pode ser vazio", nameof(name));

        _commands[name.ToLower()] = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    /// <summary>
    /// Executa um comando baseado na entrada do usuário.
    /// </summary>
    /// <param name="input">Entrada completa do usuário</param>
    /// <param name="state">Estado atual da aplicação</param>
    /// <returns>True se o comando existe e foi executado; False se comando não existe</returns>
    public async Task<bool> ExecuteAsync(string input, AppState state)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Divide o input em comando e argumentos
        var parts = input.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return false;

        var commandName = parts[0].ToLower();

        if (!_commands.ContainsKey(commandName))
        {
            state.StatusMessage = $"[yellow]Comando '{commandName}' não reconhecido. Digite 'help' para ver comandos disponíveis.[/]";
            return false;
        }

        var handler = _commands[commandName];
        var args = parts.Length > 1 ? parts.Skip(1).ToArray() : Array.Empty<string>();

        try
        {
            await handler.HandleAsync(args, state);
            return true;
        }
        catch (Exception ex)
        {
            state.StatusMessage = $"[red]Erro ao executar comando: {ex.Message}[/]";
            return false;
        }
    }

    /// <summary>
    /// Retorna todos os comandos registrados com suas descrições.
    /// </summary>
    public Dictionary<string, string> GetAllCommands()
    {
        return _commands.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Description,
            StringComparer.OrdinalIgnoreCase);
    }
}