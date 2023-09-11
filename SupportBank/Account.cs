namespace SupportBank;

public class Account
{
    public static readonly string HeaderRow = "Name".PadRight(10) + " | " + "Balance";
    public static readonly string HeaderRule = new string('-', 10) + " | " + new string('-', 7);
    public static readonly string NonePresent = "No accounts to show.";

    public string Name { get; set; }
    public decimal Balance { get; set; }

    public Account(string name, decimal balance = 0.0m)
    {
        Name = name;
        Balance = balance;
    }

    public override string ToString()
    {
        return $"{Name.PadRight(10)} | {Balance:0.00}";
    }
}
