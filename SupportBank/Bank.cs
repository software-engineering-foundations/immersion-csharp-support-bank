using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

namespace SupportBank;

public class Bank
{
    public List<Transaction> Transactions { get; set; } = new();

    public static readonly string UnableToLoadFile = "Unable to load file.";
    public static readonly string UnsupportedFileExtension =
        "Unsupported file extension (should be .csv, .json or .xml).";
    public static readonly string LoadingComplete = "Loading complete.";

    private readonly IFileSystem _fileSystem;
    private readonly ILogger<Bank> _logger;

    public Bank(IFileSystem fileSystem, ILogger<Bank> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public Bank(ILogger<Bank> logger)
        : this(new FileSystem(), logger) { }

    public void ServeCustomer()
    {
        _logger.LogInformation("Support Bank starting.");
        Console.WriteLine("Welcome to the Support Bank!");
        var shouldContinue = true;
        do
        {
            Console.WriteLine();
            Console.WriteLine("Available commands:");
            Console.WriteLine("> load <filepath>");
            Console.WriteLine("> list all");
            Console.WriteLine("> list <name>");
            Console.WriteLine("> exit");
            Console.WriteLine("Please enter a command:");
            Console.Write("> ");
            var command = Console.ReadLine() ?? string.Empty;
            if (command.StartsWith("load "))
            {
                var filepath = command["load ".Length..];
                Console.WriteLine($"Attempting to load transactions from '{filepath}'...");
                LoadTransactionsFromFile(filepath);
            }
            else if (command == "list all")
            {
                Console.WriteLine("Attempting to list all accounts...");
                ListAllAccountsAndBalances();
            }
            else if (command.StartsWith("list "))
            {
                var name = command["list ".Length..];
                Console.WriteLine($"Attempting to list all transactions for '{name}'...");
                ListAllTransactionsForAccount(name);
            }
            else if (command == "exit")
            {
                Console.WriteLine("Thanks for using the Support Bank. Goodbye!");
                shouldContinue = false;
            }
            else
            {
                Console.WriteLine("Your command was not understood. Please try again.");
            }
        } while (shouldContinue);
    }

    public void LoadTransactionsFromFile(string filepath)
    {
        var streamReader = _fileSystem.File.OpenText(filepath);
        throw new NotImplementedException();
    }

    public void ListAllAccountsAndBalances()
    {
        throw new NotImplementedException();
    }

    public void ListAllTransactionsForAccount(string name)
    {
        throw new NotImplementedException();
    }
}
