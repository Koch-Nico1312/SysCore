using System.Globalization;
using System.Text;

namespace AdminApp;

public sealed partial class AdminPortal
{
    // Kleine Daten fuer Quiz-Ergebnis.
    private readonly record struct QuizResult(int Correct, int Wrong);

    private sealed record ExpenseEntry(DateTime Date, string Category, decimal Amount, string Note);

    private void RunWorldClockTool()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ 🌍 Weltzeit                  ║");
        Console.WriteLine("╚══════════════════════════════╝");
        try
        {
            DateTime now = DateTime.Now;
            Console.WriteLine("Lokal:   " + now.ToString("dd.MM.yyyy HH:mm:ss"));
            Console.WriteLine("UTC:     " + DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm:ss"));
            var ny = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var tokyo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            Console.WriteLine("NewYork: " + TimeZoneInfo.ConvertTime(now, ny).ToString("dd.MM.yyyy HH:mm:ss"));
            Console.WriteLine("Tokio:   " + TimeZoneInfo.ConvertTime(now, tokyo).ToString("dd.MM.yyyy HH:mm:ss"));
        }
        catch (Exception e)
        {
            Console.WriteLine("fehler: " + e.Message);
        }
    }

    private void RunDiceSimulator()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ 🎲 Würfelsimulator           ║");
        Console.WriteLine("╚══════════════════════════════╝");
        Console.Write("Wie viele Würfel? ");
        string? inp = Console.ReadLine();
        if (!int.TryParse(inp, out int n) || n < 1) n = 1;
        if (n > 30) n = 30;
        Random rnd = new();
        int sum = 0;
        for (int i = 0; i < n; i++)
        {
            int x = rnd.Next(1, 7);
            sum += x;
            Console.WriteLine($"Wurf {i + 1}: {x}");
        }
        Console.WriteLine("📊 Summe: " + sum);
    }

    private void RunQuizGame()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ ❓ Mini Quiz                 ║");
        Console.WriteLine("╚══════════════════════════════╝");
        int good = 0;
        int bad = 0;

        if (AskOneQuiz("Wie viel ist 4+5?", "9")) good++; else bad++;
        if (AskOneQuiz("C# wurde von Microsoft gemacht? (ja/nein)", "ja")) good++; else bad++;
        if (AskOneQuiz("Hat ein Würfel 8 Seiten? (ja/nein)", "nein")) good++; else bad++;

        QuizResult q = new(good, bad);
        Console.WriteLine("📊 Ergebnis:");
        Console.WriteLine("✅ Richtig: " + q.Correct);
        Console.WriteLine("❌ Falsch: " + q.Wrong);
    }

    private static bool AskOneQuiz(string frage, string sol)
    {
        Console.Write(frage + " ");
        string? a = Console.ReadLine();
        string x = a?.Trim().ToLowerInvariant() ?? "";
        bool ok = x == sol;
        Console.WriteLine(ok ? "✅ korrekt" : "❌ leider falsch");
        return ok;
    }

    private void RunExpenseTracker()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ 📊 Ausgaben-Tracker          ║");
        Console.WriteLine("╚══════════════════════════════╝");
        bool end = false;
        while (!end)
        {
            Console.WriteLine("1) Liste  2) Neu  3) Monat Summe  4) Ende");
            Console.Write("> ");
            string? x = Console.ReadLine();
            string k = x?.Trim() ?? "";
            if (k == "4") end = true;
            else if (k == "1") ShowExpenseList();
            else if (k == "2") AddExpense();
            else if (k == "3") ShowExpenseMonthlySummary();
        }
    }

    private static string BuildExpenseFilePath()
    {
        return Path.Combine(SysCoreDatenVerzeichnisErmitteln(), "admin_ausgaben.txt");
    }

    private void AddExpense()
    {
        Console.Write("Kategorie: ");
        var cat = (Console.ReadLine() ?? "").Trim();
        Console.Write("Betrag (zB 12.50): ");
        var inp = Console.ReadLine();
        if (!decimal.TryParse(inp, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
        {
            Console.WriteLine("❌ Betrag ungültig");
            return;
        }
        Console.Write("Notiz: ");
        string note = Console.ReadLine() ?? "";
        var e = new ExpenseEntry(DateTime.Now, cat, amount, note);
        string line = $"{e.Date:yyyy-MM-dd}|{e.Category.Replace('|', ' ')}|{e.Amount.ToString(CultureInfo.InvariantCulture)}|{e.Note.Replace('|', ' ')}";
        File.AppendAllText(BuildExpenseFilePath(), line + Environment.NewLine, Encoding.UTF8);
        Console.WriteLine("✅ gespeichert");
    }

    private void ShowExpenseList()
    {
        var list = LoadExpensesFromFile();
        if (list.Count == 0)
        {
            Console.WriteLine("(leer)");
            return;
        }

        foreach (var e in list)
        {
            Console.WriteLine($"{e.Date:dd.MM.yyyy} | {e.Category} | {e.Amount:0.00} EUR | {e.Note}");
        }
    }

    private void ShowExpenseMonthlySummary()
    {
        var list = LoadExpensesFromFile();
        if (list.Count == 0)
        {
            Console.WriteLine("📊 keine daten");
            return;
        }

        Console.Write("Monat (1-12): ");
        string? m = Console.ReadLine();
        Console.Write("Jahr: ");
        string? y = Console.ReadLine();
        if (!int.TryParse(m, out int month) || !int.TryParse(y, out int year))
        {
            Console.WriteLine("❌ ungültig");
            return;
        }

        decimal sum = 0;
        foreach (var e in list)
        {
            if (e.Date.Month == month && e.Date.Year == year) sum += e.Amount;
        }

        Console.WriteLine($"📊 Summe {month:00}/{year}: {sum:0.00} EUR");
    }

    private List<ExpenseEntry> LoadExpensesFromFile()
    {
        var outList = new List<ExpenseEntry>();
        string path = BuildExpenseFilePath();
        if (!File.Exists(path)) return outList;

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        foreach (var raw in lines)
        {
            string s = raw.Trim();
            if (s.Length == 0) continue;
            string[] p = s.Split('|');
            if (p.Length < 4) continue;
            if (!DateTime.TryParseExact(p[0], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                continue;
            if (!decimal.TryParse(p[2], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal a))
                continue;
            outList.Add(new ExpenseEntry(dt, p[1], a, p[3]));
        }

        return outList;
    }
}
