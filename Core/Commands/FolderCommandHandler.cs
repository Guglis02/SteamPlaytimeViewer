namespace SteamPlaytimeViewer.Core.Commands;

public class FolderCommandHandler : ICommandHandler
{
    public string Description => "Define a pasta do Steam (uso: folder <caminho>), execução sem comando mostra o caminho atual";

    public Task<bool> HandleAsync(string[] args, AppState state)
    {
        if (args.Length == 0)
        {
            if (string.IsNullOrWhiteSpace(state.SteamFolder))
            {
                state.StatusMessage = "[yellow]Pasta do Steam não configurada. Use: folder <caminho>[/]";
            }
            else
            {
                state.StatusMessage = $"[cyan]Pasta Steam atual:[/] {state.SteamFolder}";
            }
            return Task.FromResult(true);
        }

        // Juntar argumentos
        var rawPath = string.Join(" ", args).Trim();
        // Remover aspas 
        rawPath = rawPath.Trim('"', '\'');
        // Normalizar o caminho para o os atual
        var normalizedPath = NormalizePath(rawPath);

        // Validar se o diretório existe
        if (!Directory.Exists(normalizedPath))
        {
            state.StatusMessage = 
                $"[red]Pasta não encontrada:[/] {normalizedPath}";
            return Task.FromResult(false);
        }

        state.SteamFolder = normalizedPath;
        state.StatusMessage = $"[green]Pasta do Steam configurada:[/] {normalizedPath}";

        return Task.FromResult(true);
    }

    /// <summary>
    /// Normaliza um caminho para usar os separadores corretos do sistema operacional.
    /// </summary>
    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        path = path.Replace('/', Path.DirectorySeparatorChar)
                   .Replace('\\', Path.DirectorySeparatorChar);

        // Remover separadores duplicados
        while (path.Contains($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}"))
        {
            path = path.Replace(
                $"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}",
                Path.DirectorySeparatorChar.ToString()
            );
        }

        path = Environment.ExpandEnvironmentVariables(path);

        // Obter caminho absoluto se for relativo
        try
        {
            path = Path.GetFullPath(path);
        }
        catch
        {
            // Se falhar, retorna o caminho original
        }

        return path;
    }
}