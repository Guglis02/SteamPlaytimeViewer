namespace SteamPlaytimeViewer.Core.Commands;

public class FolderCommandHandler : ICommandHandler
{
    public string Description => "Set Steam folder path (usage: folder <path>)";

    public Task<bool> HandleAsync(string[] args, AppState state)
    {
        if (args.Length == 0)
        {
            if (string.IsNullOrWhiteSpace(state.SteamFolder))
            {
                state.StatusMessage = "[yellow]Steam folder not configured. Use: folder <path>[/]";
            }
            else
            {
                state.StatusMessage = $"[cyan]Current Steam folder:[/] {state.SteamFolder}";
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
            state.StatusMessage = $"[red]Folder not found:[/] {normalizedPath}";
            return Task.FromResult(false);
        }

        state.SteamFolder = normalizedPath;
        state.StatusMessage = $"[green]Steam folder set:[/] {normalizedPath}";

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