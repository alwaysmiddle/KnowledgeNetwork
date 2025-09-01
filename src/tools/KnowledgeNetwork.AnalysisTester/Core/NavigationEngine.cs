using System.Reflection;
using Spectre.Console;

namespace KnowledgeNetwork.AnalysisTester.Core;

/// <summary>
/// Handles component discovery, search, and navigation within the test environment.
/// This engine finds and organizes all available test commands for easy access.
/// </summary>
public class NavigationEngine
{
    private readonly List<CommandBase> _availableCommands = [];
    private readonly Dictionary<string, List<CommandBase>> _commandsByCategory = new();
    private readonly OutputFormatter _formatter = new();

    #region Command Discovery and Registration

    /// <summary>
    /// Discover and register all available test commands
    /// </summary>
    public async Task<int> DiscoverCommandsAsync()
    {
        _formatter.ShowProgress("Discovering available test commands...");

        try
        {
            // Clear existing commands
            _availableCommands.Clear();
            _commandsByCategory.Clear();

            // Find all command implementations in the current assembly
            var commandTypes = FindCommandTypes();
            
            _formatter.ShowProgress($"Found {commandTypes.Count} potential command types");

            // Instantiate and register commands
            foreach (var type in commandTypes)
            {
                try
                {
                    var command = await CreateCommandInstance(type);
                    if (command != null)
                    {
                        RegisterCommand(command);
                    }
                }
                catch (Exception ex)
                {
                    _formatter.DisplayWarning($"Failed to create {type.Name}", ex.Message);
                }
            }

            _formatter.DisplaySuccess("Command Discovery Complete", 
                $"Registered {_availableCommands.Count} test commands across {_commandsByCategory.Count} categories");

            return _availableCommands.Count;
        }
        catch (Exception ex)
        {
            _formatter.DisplayError("Command discovery failed", ex);
            return 0;
        }
    }

    /// <summary>
    /// Manually register a command
    /// </summary>
    public void RegisterCommand(CommandBase command)
    {
        if (_availableCommands.Any(c => c.Name == command.Name))
        {
            _formatter.DisplayWarning("Duplicate Command", $"Command '{command.Name}' already registered");
            return;
        }

        _availableCommands.Add(command);

        // Group by category
        if (!_commandsByCategory.ContainsKey(command.Category))
        {
            _commandsByCategory[command.Category] = new List<CommandBase>();
        }
        _commandsByCategory[command.Category].Add(command);
    }

    #endregion

    #region Search and Navigation

    /// <summary>
    /// Search for commands by name, description, or component path
    /// </summary>
    public List<CommandBase> SearchCommands(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return _availableCommands.ToList();

        var term = searchTerm.ToLowerInvariant();

        return _availableCommands.Where(cmd =>
            cmd.Name.ToLowerInvariant().Contains(term) ||
            cmd.Description.ToLowerInvariant().Contains(term) ||
            cmd.ComponentPath.ToLowerInvariant().Contains(term) ||
            cmd.Category.ToLowerInvariant().Contains(term)
        ).ToList();
    }

    /// <summary>
    /// Get commands by category
    /// </summary>
    public List<CommandBase> GetCommandsByCategory(string category)
    {
        return _commandsByCategory.ContainsKey(category) 
            ? _commandsByCategory[category].ToList() 
            : new List<CommandBase>();
    }

    /// <summary>
    /// Get all available categories
    /// </summary>
    public List<string> GetCategories()
    {
        return _commandsByCategory.Keys.OrderBy(k => k).ToList();
    }

    /// <summary>
    /// Find command by exact name
    /// </summary>
    public CommandBase? FindCommandByName(string name)
    {
        return _availableCommands.FirstOrDefault(cmd => 
            string.Equals(cmd.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get command by component path
    /// </summary>
    public CommandBase? FindCommandByPath(string componentPath)
    {
        return _availableCommands.FirstOrDefault(cmd => 
            string.Equals(cmd.ComponentPath, componentPath, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Interactive Navigation

    /// <summary>
    /// Interactive command browser with search and selection
    /// </summary>
    public async Task<CommandBase?> BrowseCommandsInteractivelyAsync()
    {
        if (!_availableCommands.Any())
        {
            _formatter.DisplayWarning("No Commands Available", 
                "No test commands found. Run command discovery first.");
            return null;
        }

        while (true)
        {
            var choice = _formatter.PromptMainNavigation(_commandsByCategory.Keys.ToList());

            switch (choice)
            {
                case "list":
                    DisplayAllCommands();
                    break;

                case "search":
                    return await InteractiveSearch();

                case "category":
                    return await BrowseByCategory();

                case "recent":
                    return BrowseRecentCommands();

                case "exit":
                    return null;

                default:
                    _formatter.DisplayWarning("Invalid Choice", "Please select a valid option");
                    break;
            }
        }
    }

    /// <summary>
    /// Interactive search functionality
    /// </summary>
    public async Task<CommandBase?> InteractiveSearch()
    {
        var searchTerm = AnsiConsole.Ask<string>("Enter search term:");
        var results = SearchCommands(searchTerm);

        if (!results.Any())
        {
            _formatter.DisplayWarning("No Results", $"No commands found matching '{searchTerm}'");
            return null;
        }

        _formatter.DisplaySearchResults(searchTerm, results);

        if (results.Count == 1)
        {
            return results[0];
        }

        return _formatter.PromptCommandSelection(results);
    }

    /// <summary>
    /// Browse commands by category
    /// </summary>
    public async Task<CommandBase?> BrowseByCategory()
    {
        var categories = GetCategories();
        if (!categories.Any())
        {
            _formatter.DisplayWarning("No Categories", "No command categories available");
            return null;
        }

        var selectedCategory = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold blue]Select a category:[/]")
                .AddChoices(categories)
        );

        var commands = GetCommandsByCategory(selectedCategory);
        if (!commands.Any())
        {
            _formatter.DisplayWarning("Empty Category", $"No commands in category '{selectedCategory}'");
            return null;
        }

        _formatter.DisplayCommandsByCategory(selectedCategory, commands);
        return _formatter.PromptCommandSelection(commands);
    }

    /// <summary>
    /// Browse recently used commands (placeholder for now)
    /// </summary>
    public CommandBase? BrowseRecentCommands()
    {
        // TODO: Implement recent command tracking
        _formatter.DisplayInfo("Recent commands feature will be implemented in a future version");
        return null;
    }

    #endregion

    #region Display Methods

    /// <summary>
    /// Display all available commands organized by category
    /// </summary>
    public void DisplayAllCommands()
    {
        var tree = new Tree("[bold blue]📋 Available Test Commands[/]");

        foreach (var category in _commandsByCategory.Keys.OrderBy(k => k))
        {
            var categoryNode = tree.AddNode($"[bold yellow]{category}[/]");
            var commands = _commandsByCategory[category].OrderBy(c => c.Name);

            foreach (var command in commands)
            {
                var commandNode = categoryNode.AddNode($"[green]{command.Name}[/]");
                commandNode.AddNode($"[dim]{command.Description}[/]");
                commandNode.AddNode($"[dim]Path: {command.ComponentPath}[/]");
            }
        }

        AnsiConsole.Write(tree);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Display summary statistics
    /// </summary>
    public void DisplaySummary()
    {
        var table = new Table()
            .Title("[bold]🎯 Test Environment Summary[/]")
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Category[/]")
            .AddColumn("[bold]Commands[/]")
            .AddColumn("[bold]Description[/]");

        foreach (var category in _commandsByCategory.Keys.OrderBy(k => k))
        {
            var count = _commandsByCategory[category].Count;
            var description = GetCategoryDescription(category);
            
            table.AddRow(
                $"[yellow]{category}[/]",
                $"[cyan]{count}[/]",
                $"[dim]{description}[/]"
            );
        }

        table.AddRow(
            "[bold]Total[/]",
            $"[bold cyan]{_availableCommands.Count}[/]",
            "[dim]All registered commands[/]"
        );

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Find all types that inherit from CommandBase
    /// </summary>
    private List<Type> FindCommandTypes()
    {
        var commandTypes = new List<Type>();

        try
        {
            // Get all loaded assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(CommandBase)))
                        .ToList();

                    commandTypes.AddRange(types);
                }
                catch (Exception ex)
                {
                    // Skip assemblies that can't be reflected
                    _formatter.DisplayWarning($"Assembly reflection failed: {assembly.FullName}", ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _formatter.DisplayError("Type discovery failed", ex);
        }

        return commandTypes;
    }

    /// <summary>
    /// Create an instance of a command type
    /// </summary>
    private async Task<CommandBase?> CreateCommandInstance(Type type)
    {
        try
        {
            var instance = Activator.CreateInstance(type) as CommandBase;
            return instance;
        }
        catch (Exception ex)
        {
            _formatter.DisplayError($"Failed to create {type.Name}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Get description for a category
    /// </summary>
    private string GetCategoryDescription(string category)
    {
        return category switch
        {
            "Analyzers" => "Code analysis and inspection tools",
            "Controllers" => "API endpoint testing commands",
            "Models" => "Data model testing and validation",
            "Services" => "Service layer testing commands",
            "Infrastructure" => "Infrastructure component tests",
            _ => "Test commands for various components"
        };
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Get all registered commands
    /// </summary>
    public IReadOnlyList<CommandBase> AllCommands => _availableCommands.AsReadOnly();

    /// <summary>
    /// Get commands grouped by category
    /// </summary>
    public IReadOnlyDictionary<string, List<CommandBase>> CommandsByCategory => 
        _commandsByCategory.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList());

    /// <summary>
    /// Get total number of registered commands
    /// </summary>
    public int CommandCount => _availableCommands.Count;

    /// <summary>
    /// Get number of categories
    /// </summary>
    public int CategoryCount => _commandsByCategory.Count;

    #endregion
}