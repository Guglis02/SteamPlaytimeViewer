namespace SteamPlaytimeViewer.Data.Dtos;

public record GameImportDto(
    int AppId,
    string Title,
    int TotalAchievements,
    int UnlockedAchievements,
    double PlaytimeHours,
    DateTime? FirstPlayed,
    DateTime? LastPlayed
);