namespace SteamPlaytimeViewer;

public record Game(
    string Title,
    string Playtime,
    string Achievements,
    string Percentage,
    string FirstSession,
    string LastSession
);