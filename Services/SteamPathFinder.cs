using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SteamPlaytimeViewer.Services;

public static class SteamPathFinder
{
    public static string? TryAutoDetectSteamPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWindowsSteamPath();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return GetLinuxSteamPath();
        }
        
        return null;
    }

    private static string? GetWindowsSteamPath()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            if (key?.GetValue("SteamPath") is string registryPath)
            {
                string path = registryPath.Replace('/', Path.DirectorySeparatorChar);
                
                if (Directory.Exists(path)) return path;
            }
        }
        catch
        {
            // Ignora erros de permissão/registro e tenta o padrão
        }

        // Fallback para o caminho padrão 64-bit
        string defaultPath64 = @"C:\Program Files (x86)\Steam";
        if (Directory.Exists(defaultPath64)) return defaultPath64;

        // Fallback para o caminho padrão 32-bit
        string defaultPath32 = @"C:\Program Files\Steam";
        if (Directory.Exists(defaultPath32)) return defaultPath32;

        return null;
    }

    private static string? GetLinuxSteamPath()
    {
        string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        string[] commonPaths = 
        {
            Path.Combine(homeDir, ".steam", "steam"),
            
            Path.Combine(homeDir, ".local", "share", "Steam"),
            
            Path.Combine(homeDir, ".var", "app", "com.valvesoftware.Steam", ".steam", "steam")
        };

        foreach (var path in commonPaths)
        {
            if (Directory.Exists(path)) return path;
        }

        return null;
    }
}