using Spectre.Console;
using SteamPlaytimeViewer.Core;

namespace SteamPlaytimeViewer;

public static class UserInterface
{
    static string[] NoDataRow(int size)
    {
        if (size == 0)
        {
            return Array.Empty<string>();
        }

        string[] row = new string[size];
        row[0] = "[grey]No data[/]";
        for (int i = 1; i < size; i++)
        {
            row[i] = $"-";
        }

        return row;
    }

    public static Layout BuildLayout(UserInfo user, List<GameView> visibleGames, int totalGames, int scrollIndex, string input, string statusMessage)
    {
        var panelHeader = $"  Profile: {user.Username} - SteamId64: {user.SteamId}  ";

        var table = new Table()
            .Border(TableBorder.Horizontal)
            .BorderColor(Color.Teal)
            .Expand();

        table.AddColumn("Title", col => col.NoWrap().Alignment(Justify.Left));
        table.AddColumn("Playtime", col => col.NoWrap().Alignment(Justify.Right));
        table.AddColumn("Achievements", col => col.NoWrap().Alignment(Justify.Right));
        table.AddColumn("%", col => col.NoWrap().Alignment(Justify.Right));
        table.AddColumn("First_Session", col => col.NoWrap().Alignment(Justify.Right));
        table.AddColumn("Last_Session", col => col.NoWrap().Alignment(Justify.Right));

        if (visibleGames.Any())
        {
            foreach (var game in visibleGames)
            {
                table.AddRow(
                    game.Title,
                    game.Playtime,
                    game.Achievements,
                    game.Percentage,
                    game.FirstSession,
                    game.LastSession);
            }
        }
        else
        {
            table.AddRow(NoDataRow(table.Columns.Count));
        }

        var infoScroll = totalGames > 0
            ? $"[grey]Showing {scrollIndex + 1}-{scrollIndex + visibleGames.Count} of {totalGames} (Use ↑/↓)[/]"
            : "";

        table.Caption(infoScroll);

        var mainPanel = new Panel(Align.Center(table))
            .Header(panelHeader)
            .HeaderAlignment(Justify.Left)
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Teal)
            .Padding(1, 2, 1, 0)
            .Expand();

        var inputPanel = new Panel(new Markup($"[green]>[/] {input}[blink]_[/]"))
            .Header("Command")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Teal)
            .Padding(1, 0, 1, 0)
            .Expand();

        var statusPanel = new Panel(new Markup(statusMessage))
            .Padding(1, 0, 1, 0)
            .Expand();

        var layout = new Layout("Root")
            .SplitRows(
                new Layout("Data").Ratio(5),
                new Layout("Status").Size(3),
                new Layout("Footer").Size(3)
            );

        layout["Data"].Update(mainPanel);
        layout["Status"].Update(statusPanel);
        layout["Footer"].Update(inputPanel);

        return layout;
    }
}