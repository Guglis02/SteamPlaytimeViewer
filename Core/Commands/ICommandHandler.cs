namespace SteamPlaytimeViewer.Core.Commands;

/// <summary>
/// Interface que define um handler de comando.
/// </summary>
public interface ICommandHandler
{
    /// <summary>
    /// Processa o comando com os argumentos fornecidos.
    /// </summary>
    /// <param name="args">Argumentos do comando após o nome do comando</param>
    /// <param name="state">Estado atual da aplicação</param>
    /// <returns>True se o comando foi processado com sucesso</returns>
    Task<bool> HandleAsync(string[] args, AppState state);

    /// <summary>
    /// Descrição do comando para exibição no help.
    /// </summary>
    string Description { get; }
}