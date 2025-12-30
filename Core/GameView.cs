namespace SteamPlaytimeViewer.Core;

public record GameView(
    string Title,
    string Playtime,
    string Achievements,
    string Percentage,
    string FirstSession,
    string LastSession
);