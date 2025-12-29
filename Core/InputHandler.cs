using SteamPlaytimeViewer.Core.Commands;

namespace SteamPlaytimeViewer.Core;

/// <summary>
/// Processa entrada do teclado do usuário.
/// </summary>
public class InputHandler
{
    private readonly CommandRegistry _commandRegistry;

    public InputHandler(CommandRegistry commandRegistry)
    {
        _commandRegistry = commandRegistry ?? throw new ArgumentNullException(nameof(commandRegistry));
    }

    /// <summary>
    /// Processa uma tecla pressionada pelo usuário.
    /// </summary>
    /// <param name="key">Informação da tecla pressionada</param>
    /// <param name="state">Estado atual da aplicação</param>
    /// <returns>True se a entrada foi processada</returns>
    public async Task<bool> ProcessInputAsync(ConsoleKeyInfo key, AppState state)
    {
        state.MarkDirty();

        // Navegação vertical
        if (key.Key == ConsoleKey.UpArrow)
        {
            if (state.ScrollIndex > 0)
                state.ScrollIndex--;
            return true;
        }

        if (key.Key == ConsoleKey.DownArrow)
        {
            int maxScroll = Math.Max(0, state.AllGames.Count - state.ItemsPerPage);
            if (state.ScrollIndex < maxScroll)
                state.ScrollIndex++;
            return true;
        }

        // Apaga ultimo caracter do buffer de entrada
        if (key.Key == ConsoleKey.Backspace)
        {
            if (state.InputBuffer.Length > 0)
                state.InputBuffer.Length--;
            return true;
        }

        // Execução de comando ao pressionar Enter
        if (key.Key == ConsoleKey.Enter)
        {
            var command = state.InputBuffer.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(command))
            {
                await _commandRegistry.ExecuteAsync(command, state);
            }
            state.InputBuffer.Clear();
            return true;
        }

        // Adiciona caracteres normais ao buffer
        if (!char.IsControl(key.KeyChar))
        {
            state.InputBuffer.Append(key.KeyChar);
            return true;
        }

        return false;
    }
}