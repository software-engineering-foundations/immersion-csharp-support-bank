namespace SupportBank;

public class Transaction
{
    public static readonly string HeaderRow =
        "Date".PadRight(10)
        + " | "
        + "From".PadRight(10)
        + " | "
        + "To".PadRight(10)
        + " | "
        + "Narrative".PadRight(35)
        + " | "
        + "Amount";

    public static readonly string HeaderRule =
        new string('-', 10)
        + " | "
        + new string('-', 10)
        + " | "
        + new string('-', 10)
        + " | "
        + new string('-', 35)
        + " | "
        + new string('-', 6);

    public static readonly string NonePresent = "No transactions to show.";

    public DateOnly Date { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public string Narrative { get; set; }
    public decimal Amount { get; set; }

    public Transaction(DateOnly date, string from, string to, string narrative, decimal amount)
    {
        Date = date;
        From = from;
        To = to;
        Narrative = narrative;
        Amount = amount;
    }

    public override string ToString()
    {
        return $"{Date} | {From.PadRight(10)} | {To.PadRight(10)} | {Narrative.PadRight(35)} | {Amount:0.00}";
    }
}
