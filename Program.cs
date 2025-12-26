using System.Text;
using Spectre.Console;
using SteamPlaytimeViewer;

public class Program
{
    
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        AnsiConsole.Clear();
        
        var dataService = new DataService();

        var inputBuffer = new StringBuilder();
        var currentUser = "Guglis";
        
        string helpMessage = "Type 'user <name>' to change profile or 'exit'.";
        string statusMessage = helpMessage;

        int scrollIndex = 0;

        var initialLayout = UserInterface.BuildLayout(currentUser, new List<Game>(), 0, 0, "", "");
        
        AnsiConsole.Live(initialLayout)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Start(context =>
            {
                while (true)
                {
                    var allGames = dataService.GetGames(currentUser);
                 
                    int terminalHeight = Console.WindowHeight;
                    int itemsPerPage = terminalHeight - 15;
                    
                    var visibleGames = allGames.Skip(scrollIndex).Take(itemsPerPage).ToList();
                    
                    var layout = UserInterface.BuildLayout(
                        currentUser,
                        visibleGames,
                        allGames.Count,
                        scrollIndex,
                        inputBuffer.ToString(),
                        statusMessage);

                    context.UpdateTarget(layout);
                    
                    var readKey = Console.ReadKey(intercept: true);
                    
                    if (readKey.Key == ConsoleKey.UpArrow)
                    {
                        if (scrollIndex > 0) scrollIndex--;
                    }
                    else if (readKey.Key == ConsoleKey.DownArrow)
                    {
                        if (scrollIndex < allGames.Count - itemsPerPage) scrollIndex++;
                    }
                    else if (readKey.Key == ConsoleKey.Backspace)
                    {
                        if (inputBuffer.Length > 0)
                        {
                            inputBuffer.Length--;
                        }
                    }
                    else if (readKey.Key == ConsoleKey.Enter)
                    {
                        var command = inputBuffer.ToString().Trim();
                        inputBuffer.Clear();

                        if (command == "exit")
                        {
                            break;
                        }
                        else if (command == "help")
                        {
                            statusMessage = helpMessage;
                        }
                        else if (command.StartsWith("user "))
                        {
                            var newUser = command.Substring(5).Trim();
                            
                            if (dataService.HaveUser(newUser))
                            {
                                currentUser = newUser;
                                scrollIndex = 0;
                                statusMessage = $"[green]User changed to {newUser}![/]";
                            }
                            else
                            {
                                statusMessage = $"[red]User '{newUser}' not found![/]";
                            }
                        }
                        else
                        {
                            statusMessage = $"[yellow]Unknown command '{command}'![/]";
                        }
                    }
                    else if (!char.IsControl(readKey.KeyChar))
                    {
                        inputBuffer.Append(readKey.KeyChar);
                    }
                }
            });
    }

    
}
