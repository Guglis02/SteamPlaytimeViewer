using SteamPlaytimeViewer.Services;

namespace SteamPlaytimeViewer.Core.Commands;

public class UserCommandHandler : ICommandHandler
{
    private readonly DataService _dataService;

    public UserCommandHandler(DataService dataService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
    }

    public string Description => "Change user/profile (usage: user <username|steamid|url>)";

    public async Task<bool> HandleAsync(string[] args, AppState state)
    {
        if (args.Length == 0)
        {
            state.StatusMessage = "[yellow]Usage: user <username>, user <url> or user <steamid>[/]";
            return false;
        }

        var userInput = string.Join(" ", args).Trim();

        // Detectar tipo de input
        var inputType = DetectInputType(userInput, out var extractedValue);
        UserInfo? userInfo = null;

        switch (inputType)
        {
            case InputType.Direct64BitSteamId:
                userInfo = await HandleSteamIdAsync(extractedValue, state);
                break;

            case InputType.ProfilesUrl:
                userInfo = await HandleSteamIdAsync(extractedValue, state);
                break;

            case InputType.VanityUrl:
                userInfo = await HandleVanityUrlAsync(extractedValue, state);
                break;

            case InputType.Username:
                userInfo = await HandleUsernameAsync(extractedValue, state);
                break;
        }

        if (userInfo == null)
            return false;

        // Atualizar estado
        state.CurrentUser = userInfo;
        state.ScrollIndex = 0;
        state.SearchQuery = null; // Limpar busca anterior
        state.SortColumn = nameof(GameView.Title); // Reset ordenação
        state.SortAscending = true;
        state.ShouldUpdateList = true;

        state.StatusMessage = $"[green]User changed to {userInfo.Username}![/]";
        return true;
    }

    /// <summary>
    /// Detecta o tipo de entrada fornecida pelo usuário e extrai o valor relevante.
    /// Identifica se é um SteamID direto, URL com /profiles/, URL customizada com /id/, ou um nickname registrado.
    /// </summary>
    /// <param name="input">A entrada do usuário (SteamID, URL ou nickname)</param>
    /// <param name="extractedValue">Saída com o valor extraído (SteamID ou nome de usuário)</param>
    /// <returns>O tipo de entrada detectado</returns>
    private InputType DetectInputType(string input, out string extractedValue)
    {
        input = input.Trim();
        extractedValue = input;

        // Verifica se é um ID numérico direto
        if (ulong.TryParse(input, out _) && input.Length >= 17)
        {
            return InputType.Direct64BitSteamId;
        }

        // Verifica se é URL com /profiles/
        if (input.Contains("/profiles/"))
        {
            var parts = input.Split(new[] { "/profiles/" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
            {
                var steamId = parts[1].Split('/')[0];
                if (ulong.TryParse(steamId, out _) && steamId.Length >= 17)
                {
                    extractedValue = steamId;
                    return InputType.ProfilesUrl;
                }
            }
        }

        // Verifica se é URL customizada (/id/)
        if (input.Contains("/id/"))
        {
            var parts = input.Split(new[] { "/id/" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
            {
                extractedValue = parts[1].Split('/')[0];
                return InputType.VanityUrl;
            }
        }

        // Se parecer uma URL com /id/ mesmo sem estar formatado corretamente
        if (input.Contains("/"))
        {
            // Possível URL, tenta extrair a parte final
            var parts = input.Split('/');
            if (parts.Length > 0)
            {
                var lastPart = parts[^1];
                if (!string.IsNullOrWhiteSpace(lastPart) && !lastPart.Contains("."))
                {
                    extractedValue = lastPart;
                    return InputType.VanityUrl;
                }
            }
        }

        // Se nada acima funcionou, tratar como username
        return InputType.Username;
    }

    private async Task<UserInfo?> HandleSteamIdAsync(string steamId, AppState state)
    {
        try
        {
            var userInfo = await _dataService.ResolveBySteamIdAsync(steamId);

            if (userInfo == null)
            {
                state.StatusMessage = $"[red]SteamID '{steamId}' not found in Steam API.[/]";
                return null;
            }

            return userInfo;
        }
        catch (Exception ex)
        {
            state.StatusMessage = $"[red]Error: {ex.Message}[/]";
            return null;
        }
    }

    private async Task<UserInfo?> HandleVanityUrlAsync(string vanityName, AppState state)
    {
        try
        {
            var steamId = await _dataService.ResolveVanityUrlAsync(vanityName);

            if (string.IsNullOrEmpty(steamId))
            {
                state.StatusMessage = $"[yellow]Perfil '{vanityName}' não encontrado na Steam.[/]";
                return null;
            }

            var userInfo = await _dataService.ResolveBySteamIdAsync(steamId);

            if (userInfo == null)
            {
                state.StatusMessage = $"[red]SteamID '{steamId}' not found in Steam API.[/]";
                return null;
            }

            return userInfo;
        }
        catch (Exception ex)
        {
            state.StatusMessage = $"[red]Error: {ex.Message}[/]";
            return null;
        }
    }

    private async Task<UserInfo?> HandleUsernameAsync(string username, AppState state)
    {
        var userInfo = await _dataService.ResolveByUsernameAsync(username);

        if (userInfo == null)
        {
            state.StatusMessage =
                $"[yellow]Usuário '{username}' não encontrado no banco. Use o SteamID para registrar um novo usuário.[/]";
            return null;
        }

        return userInfo;
    }
}

public enum InputType
{
    Direct64BitSteamId,
    ProfilesUrl,
    VanityUrl,
    Username
}