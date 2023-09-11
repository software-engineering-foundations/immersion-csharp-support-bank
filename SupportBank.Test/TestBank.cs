using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SupportBank.Test;

public class TestBank
{
    private static readonly string[] AccountHolders =
    {
        "Alice S",
        "Bob J",
        "Charlie T",
        "David B",
        "Eve W",
        "Frank W",
    };

    private static readonly List<Transaction> Transactions =
        new()
        {
            new Transaction(
                new DateOnly(2019, 8, 6),
                AccountHolders[0],
                AccountHolders[1],
                "Apples",
                10.00m
            ),
            new Transaction(
                new DateOnly(2020, 9, 10),
                AccountHolders[1],
                AccountHolders[2],
                "Cherries",
                3.50m
            ),
            new Transaction(
                new DateOnly(2021, 10, 20),
                AccountHolders[2],
                AccountHolders[3],
                "Grapes",
                0.45m
            ),
            new Transaction(
                new DateOnly(2022, 11, 25),
                AccountHolders[2],
                AccountHolders[4],
                "Lemons",
                8.00m
            ),
            new Transaction(
                new DateOnly(2023, 12, 30),
                AccountHolders[5],
                AccountHolders[4],
                "Peaches",
                5.00m
            ),
        };

    public static readonly IEnumerable<object[]> CsvFileData = new List<object[]>
    {
        new object[] { "test.csv", Transactions },
    };

    public static readonly IEnumerable<object[]> JsonFileData = new List<object[]>
    {
        new object[] { "test.json", Transactions },
    };

    public static readonly IEnumerable<object[]> XmlFileData = new List<object[]>
    {
        new object[] { "test.xml", Transactions },
    };

    [Theory]
    [MemberData(nameof(CsvFileData))]
    public void LoadTransactionsFromFile_CalledWithCsvFile_LoadsExpectedTransactions(
        string filepath,
        List<Transaction> expectedTransactions
    )
    {
        // Arrange
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var fileContent =
            "Date,From,To,Narrative,Amount\n"
            + string.Join(
                '\n',
                expectedTransactions.Select(
                    transaction =>
                        $"{transaction.Date.ToString("dd/MM/yyyy")},{transaction.From},{transaction.To},"
                        + $"{(transaction.Narrative.Contains(',') ? '"' + transaction.Narrative + '"' : transaction.Narrative)},"
                        + $"{transaction.Amount}"
                )
            );
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { { filepath, new MockFileData(fileContent) }, }
        );
        var bank = new Bank(fileSystem, new NullLogger<Bank>());

        // Act
        bank.LoadTransactionsFromFile(filepath);

        // Assert
        bank.Transactions.Should().BeEquivalentTo(expectedTransactions);
        var outputLines = stringWriter
            .ToString()
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        outputLines.Length.Should().Be(1);
        outputLines[0].Should().Be(Bank.LoadingComplete);
    }

    [Theory]
    [MemberData(nameof(CsvFileData))]
    public void LoadTransactionsFromFile_CalledWithDodgyCsvFile_LoadsExpectedTransactionsAndLogsErrors(
        string filepath,
        List<Transaction> expectedTransactions
    )
    {
        // Arrange
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var fileContent =
            "Date,From,To,Narrative,Amount\n"
            + $"30/30/2015,{AccountHolders[3]},{AccountHolders[0]},Raspberries,2.50\n"
            + $"01/01/2016,{AccountHolders[1]},{AccountHolders[4]},Strawberries,Five Strawberries\n"
            + string.Join(
                '\n',
                expectedTransactions.Select(
                    transaction =>
                        $"{transaction.Date.ToString("dd/MM/yyyy")},{transaction.From},{transaction.To},"
                        + $"{(transaction.Narrative.Contains(',') ? '"' + transaction.Narrative + '"' : transaction.Narrative)},"
                        + $"{transaction.Amount}"
                )
            );
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { { filepath, new MockFileData(fileContent) }, }
        );
        var logger = A.Fake<ILogger<Bank>>();
        var bank = new Bank(fileSystem, logger);

        // Act
        bank.LoadTransactionsFromFile(filepath);

        // Assert
        bank.Transactions.Should().BeEquivalentTo(expectedTransactions);
        var outputLines = stringWriter
            .ToString()
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        outputLines.Length.Should().Be(3);
        outputLines[0].Should().StartWith("Unable to parse transaction");
        outputLines[1].Should().StartWith("Unable to parse transaction");
        outputLines[2].Should().Be(Bank.LoadingComplete);
        A.CallTo(logger)
            .Where(
                call =>
                    call.Method.Name == nameof(logger.Log)
                    && call.GetArgument<LogLevel>(0) == LogLevel.Error
            )
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public void ListAllAccountsAndBalances_CalledWhenAtLeastOneTransactionExists_OutputsExpectedAccounts()
    {
        // Arrange
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var bank = new Bank(new FileSystem(), new NullLogger<Bank>());
        bank.Transactions.AddRange(Transactions);

        // Act
        bank.ListAllAccountsAndBalances();

        // Assert
        var outputLines = stringWriter
            .ToString()
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        outputLines.Length.Should().Be(2 + AccountHolders.Length);
        outputLines[0].Should().Be(Account.HeaderRow);
        outputLines[1].Should().Be(Account.HeaderRule);
        outputLines
            .Should()
            .Contain(
                new Account(
                    AccountHolders[1],
                    Transactions[0].Amount - Transactions[1].Amount
                ).ToString()
            );
        outputLines
            .Should()
            .Contain(
                new Account(
                    AccountHolders[2],
                    Transactions[1].Amount - Transactions[2].Amount - Transactions[3].Amount
                ).ToString()
            );
    }

    [Fact]
    public void ListAllAccountsAndBalances_CalledWhenNoTransactionsExist_OutputsExpectedMessageAndLogsWarning()
    {
        // Arrange
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var logger = A.Fake<ILogger<Bank>>();
        var bank = new Bank(new FileSystem(), logger);

        // Act
        bank.ListAllAccountsAndBalances();

        // Assert
        var outputLines = stringWriter
            .ToString()
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        outputLines.Length.Should().Be(1);
        outputLines[0].Should().Be(Account.NonePresent);
        A.CallTo(logger)
            .Where(
                call =>
                    call.Method.Name == nameof(logger.Log)
                    && call.GetArgument<LogLevel>(0) == LogLevel.Warning
            )
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void ListAllTransactionsForAccount_CalledWhenAccountExists_OutputsExpectedTransactions()
    {
        // Arrange
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var bank = new Bank(new FileSystem(), new NullLogger<Bank>());
        bank.Transactions.AddRange(Transactions);
        var selectedAccountHolder = AccountHolders[2];
        var relevantTransactions = Transactions
            .Where(
                transaction =>
                    transaction.To == selectedAccountHolder
                    || transaction.From == selectedAccountHolder
            )
            .ToList();

        // Act
        bank.ListAllTransactionsForAccount(selectedAccountHolder);

        // Assert
        var outputLines = stringWriter
            .ToString()
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        outputLines.Length.Should().Be(2 + relevantTransactions.Count);
        outputLines[0].Should().Be(Transaction.HeaderRow);
        outputLines[1].Should().Be(Transaction.HeaderRule);
        outputLines.Should().Contain(relevantTransactions.First().ToString());
        outputLines.Should().Contain(relevantTransactions.Last().ToString());
    }

    [Fact]
    public void ListAllTransactionsForAccount_CalledWhenAccountDoesntExist_OutputsExpectedMessageAndLogsWarning()
    {
        // Arrange
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var logger = A.Fake<ILogger<Bank>>();
        var bank = new Bank(new FileSystem(), logger);
        bank.Transactions.AddRange(Transactions);
        var selectedAccountHolder = "Mallory W";

        // Act
        bank.ListAllTransactionsForAccount(selectedAccountHolder);

        // Assert
        var outputLines = stringWriter
            .ToString()
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        outputLines.Length.Should().Be(1);
        outputLines[0].Should().Be(Transaction.NonePresent);
        A.CallTo(logger)
            .Where(
                call =>
                    call.Method.Name == nameof(logger.Log)
                    && call.GetArgument<LogLevel>(0) == LogLevel.Warning
            )
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [MemberData(nameof(JsonFileData))]
    public void LoadTransactionsFromFile_CalledWithJsonFile_LoadsExpectedTransactions(
        string filepath,
        List<Transaction> expectedTransactions
    )
    {
        // Arrange
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var fileContent =
            '['
            + string.Join(
                ',',
                expectedTransactions.Select(
                    transaction =>
                        $"{{\"date\":\"{transaction.Date.ToString("yyyy-MM-dd")}\","
                        + $"\"fromAccount\":\"{transaction.From}\",\"toAccount\":\"{transaction.To}\","
                        + $"\"narrative\":\"{transaction.Narrative}\",\"amount\":{transaction.Amount}}}"
                )
            )
            + ']';
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { { filepath, new MockFileData(fileContent) }, }
        );
        var bank = new Bank(fileSystem, new NullLogger<Bank>());

        // Act
        bank.LoadTransactionsFromFile(filepath);

        // Assert
        bank.Transactions.Should().BeEquivalentTo(expectedTransactions);
        var outputLines = stringWriter
            .ToString()
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        outputLines.Length.Should().Be(1);
        outputLines[0].Should().Be(Bank.LoadingComplete);
    }

    [Theory]
    [MemberData(nameof(XmlFileData))]
    public void LoadTransactionsFromFile_CalledWithXmlFile_LoadsExpectedTransactions(
        string filepath,
        List<Transaction> expectedTransactions
    )
    {
        // Arrange
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var fileContent =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><TransactionList>"
            + string.Join(
                "",
                expectedTransactions.Select(
                    transaction =>
                        $"<SupportTransaction Date=\"{Convert.ToInt32(transaction.Date.ToDateTime(TimeOnly.MinValue).ToOADate())}\">"
                        + $"<Description>{transaction.Narrative}</Description><Value>{transaction.Amount}</Value>"
                        + $"<Parties><From>{transaction.From}</From><To>{transaction.To}</To></Parties></SupportTransaction>"
                )
            )
            + "</TransactionList>";
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { { filepath, new MockFileData(fileContent) }, }
        );
        var bank = new Bank(fileSystem, new NullLogger<Bank>());

        // Act
        bank.LoadTransactionsFromFile(filepath);

        // Assert
        bank.Transactions.Should().BeEquivalentTo(expectedTransactions);
        var outputLines = stringWriter
            .ToString()
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        outputLines.Length.Should().Be(1);
        outputLines[0].Should().Be(Bank.LoadingComplete);
    }

    [Fact]
    public void LoadTransactionsFromFile_CalledWithCSharpFile_OutputsExpectedMessageAndLogsError()
    {
        // Arrange
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var filepath = "test.cs";
        var fileContent = "Console.WriteLine(\"Hello world!\");";
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { { filepath, new MockFileData(fileContent) }, }
        );
        var logger = A.Fake<ILogger<Bank>>();
        var bank = new Bank(fileSystem, logger);

        // Act
        bank.LoadTransactionsFromFile(filepath);

        // Assert
        var outputLines = stringWriter
            .ToString()
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        outputLines.Length.Should().Be(1);
        outputLines[0].Should().Be(Bank.UnsupportedFileExtension);
        A.CallTo(logger)
            .Where(
                call =>
                    call.Method.Name == nameof(logger.Log)
                    && call.GetArgument<LogLevel>(0) == LogLevel.Error
            )
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void LoadTransactionsFromFile_CalledWithNonexistentFile_OutputsExpectedMessageAndLogsError()
    {
        // Arrange
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        var filepath = "test.csv";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
        var logger = A.Fake<ILogger<Bank>>();
        var bank = new Bank(fileSystem, logger);

        // Act
        bank.LoadTransactionsFromFile(filepath);

        // Assert
        var outputLines = stringWriter
            .ToString()
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        outputLines.Length.Should().Be(1);
        outputLines[0].Should().Be(Bank.UnableToLoadFile);
        A.CallTo(logger)
            .Where(
                call =>
                    call.Method.Name == nameof(logger.Log)
                    && call.GetArgument<LogLevel>(0) == LogLevel.Error
            )
            .MustHaveHappenedOnceExactly();
    }
}
