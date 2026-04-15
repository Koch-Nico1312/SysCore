using System.Data;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using QRCoder;

namespace AdminApp;

public sealed partial class AdminPortal
{
    // Standard-Rechner und kleine Tools.
    private void RunUnitConverter()
    {
        Console.WriteLine("=== Einheitenrechner ===");
        bool fertig = false;
        while (!fertig)
        {
            Console.WriteLine("1) km → m   2) m → km   3) kg → lb   4) lb → kg   5) °C → °F   6) °F → °C   0) Ende");
            Console.Write("> ");
            string? w = Console.ReadLine();
            string k = w?.Trim() ?? "";
            if (k == "0")
            {
                fertig = true;
                continue;
            }

            Console.Write("Wert: ");
            string? z = Console.ReadLine();
            if (!double.TryParse(z, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                continue;

            if (k == "1")
                ConvertKmToM(v);
            else if (k == "2")
                ConvertMToKm(v);
            else if (k == "3")
                ConvertKgToLb(v);
            else if (k == "4")
                ConvertLbToKg(v);
            else if (k == "5")
                ConvertCelsiusToFahrenheit(v);
            else if (k == "6")
                ConvertFahrenheitToCelsius(v);
        }
    }

    // Kilometer in Meter.
    private static void ConvertKmToM(double km)
    {
        Console.WriteLine((km * 1000.0).ToString("0.###", CultureInfo.InvariantCulture) + " m");
    }

    // Meter in Kilometer.
    private static void ConvertMToKm(double m)
    {
        Console.WriteLine((m / 1000.0).ToString("0.######", CultureInfo.InvariantCulture) + " km");
    }

    // Kilogramm in Pound.
    private static void ConvertKgToLb(double kg)
    {
        Console.WriteLine((kg * 2.2046226218).ToString("0.###", CultureInfo.InvariantCulture) + " lb");
    }

    // Pound in Kilogramm.
    private static void ConvertLbToKg(double lb)
    {
        Console.WriteLine((lb / 2.2046226218).ToString("0.###", CultureInfo.InvariantCulture) + " kg");
    }

    // Celsius in Fahrenheit.
    private static void ConvertCelsiusToFahrenheit(double c)
    {
        double f = c * 9.0 / 5.0 + 32.0;
        Console.WriteLine(f.ToString("0.###", CultureInfo.InvariantCulture) + " °F");
    }

    // Fahrenheit in Celsius.
    private static void ConvertFahrenheitToCelsius(double f)
    {
        double c = (f - 32.0) * 5.0 / 9.0;
        Console.WriteLine(c.ToString("0.###", CultureInfo.InvariantCulture) + " °C");
    }

    // Taschenrechner mit Textausdruck und Verlauf.
    private void RunCalculatorWithHistory()
    {
        Console.WriteLine("=== Taschenrechner (Ausdruck, z. B. 3+4*2) ===");
        List<string> verlauf = [];
        bool fertig = false;
        while (!fertig)
        {
            Console.Write("> ");
            string? roh = Console.ReadLine();
            string ausdruck = roh?.Trim() ?? "";
            if (ausdruck.Equals("ende", StringComparison.OrdinalIgnoreCase) || ausdruck == "0")
            {
                fertig = true;
                continue;
            }

            if (ausdruck.Equals("verlauf", StringComparison.OrdinalIgnoreCase))
            {
                PrintCalculatorHistory(verlauf);
                continue;
            }

            if (ausdruck.Length == 0)
                continue;

            string? ergebnisText = EvaluateCalculatorExpression(ausdruck);
            if (ergebnisText == null)
            {
                Console.WriteLine("Ungültiger Ausdruck.");
                continue;
            }

            Console.WriteLine("= " + ergebnisText);
            verlauf.Add(ausdruck + " = " + ergebnisText);
        }
    }

    // Gibt alle bisherigen Rechnungen aus.
    private static void PrintCalculatorHistory(List<string> history)
    {
        if (history.Count == 0)
        {
            Console.WriteLine("(leer)");
            return;
        }

        foreach (string z in history)
            Console.WriteLine(z);
    }

    // WICHTIG:
    // DataTable.Compute ist eine eingebaute .NET-Funktion, die einen Mathe-Ausdruck als Text auswertet.
    // Das ist eher fortgeschritten und nicht der typische Anfänger-Weg.
    // Wir behalten es hier, damit das Verhalten exakt gleich bleibt.
    private static string? EvaluateCalculatorExpression(string expression)
    {
        string norm = expression.Replace(',', '.');
        object? ergebnis = new DataTable().Compute(norm, null);
        if (ergebnis == null || ergebnis == DBNull.Value)
            return null;
        if (ergebnis is double d)
            return d.ToString(CultureInfo.InvariantCulture);
        if (ergebnis is float f)
            return f.ToString(CultureInfo.InvariantCulture);
        if (ergebnis is decimal m)
            return m.ToString(CultureInfo.InvariantCulture);
        if (ergebnis is int i)
            return i.ToString(CultureInfo.InvariantCulture);
        if (ergebnis is long l)
            return l.ToString(CultureInfo.InvariantCulture);
        return ergebnis.ToString();
    }

    // Caesar-Dialog: Modus wählen, Verschiebung prüfen, Text umwandeln.
    private void RunCaesarTool()
    {
        Console.WriteLine("=== Caesar ===");
        Console.Write("1=verschlüsseln 2=entschlüsseln: ");
        string? mod = Console.ReadLine();
        Console.Write("Verschiebung (1-25): ");
        string? sz = Console.ReadLine();
        if (!int.TryParse(sz, out int shift) || shift < 1 || shift > 25)
            return;
        Console.WriteLine("Text:");
        string? t = Console.ReadLine();
        string text = t ?? "";
        bool enc = mod?.Trim() == "1";
        string outText = TransformCaesarText(text, shift, enc);
        Console.WriteLine(outText);
    }

    // Führt die Caesar-Verschiebung für jeden Buchstaben durch.
    private static string TransformCaesarText(string text, int shift, bool verschluesseln)
    {
        int s = shift;
        if (!verschluesseln)
        {
            s = 26 - shift;
        }
        StringBuilder sb = new();
        foreach (char c in text)
        {
            if (c is >= 'a' and <= 'z')
            {
                int o = c - 'a';
                o = (o + s) % 26;
                sb.Append((char)('a' + o));
            }
            else if (c is >= 'A' and <= 'Z')
            {
                int o = c - 'A';
                o = (o + s) % 26;
                sb.Append((char)('A' + o));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    // Liest mehrzeiligen Text ein und zeigt einfache Kennzahlen.
    private void RunTextAnalyzer()
    {
        Console.WriteLine("=== Text Analyzer ===");
        Console.WriteLine("Text (leere Zeile beendet):");
        StringBuilder sb = new();
        while (true)
        {
            string? zeile = Console.ReadLine();
            if (zeile == null)
                break;
            if (zeile.Length == 0)
                break;
            sb.AppendLine(zeile);
        }

        string voll = sb.ToString();
        int woerter = TextAnalyzerWoerterZaehlen(voll);
        int zeichenMitLeer = voll.Length;
        int zeichenOhneLeer = TextAnalyzerZeichenOhneLeerzeichenZaehlen(voll);
        int saetze = TextAnalyzerSaetzeZaehlen(voll);
        Console.WriteLine("Wörter: " + woerter);
        Console.WriteLine("Zeichen (mit Leer): " + zeichenMitLeer);
        Console.WriteLine("Zeichen (ohne Leer): " + zeichenOhneLeer);
        Console.WriteLine("Sätze: " + saetze);
    }

    // Zählt Wörter über Leerzeichen-Erkennung.
    private static int TextAnalyzerWoerterZaehlen(string text)
    {
        int n = 0;
        bool inWort = false;
        foreach (char c in text)
        {
            if (char.IsWhiteSpace(c))
            {
                inWort = false;
            }
            else if (!inWort)
            {
                inWort = true;
                n++;
            }
        }

        return n;
    }

    // Zählt Satzenden über . ! ?
    private static int TextAnalyzerSaetzeZaehlen(string text)
    {
        int n = 0;
        foreach (char c in text)
        {
            if (c == '.' || c == '!' || c == '?')
                n++;
        }

        if (n == 0 && text.Trim().Length > 0)
            return 1;
        return n;
    }

    // Ohne LINQ: Zeichen zählen, die keine Leerzeichen sind.
    private static int TextAnalyzerZeichenOhneLeerzeichenZaehlen(string text)
    {
        int anzahl = 0;
        foreach (char c in text)
        {
            if (!char.IsWhiteSpace(c))
                anzahl++;
        }

        return anzahl;
    }

    // Nutzt eine Online-API für Wechselkurse.
    private void RunCurrencyConverter()
    {
        Console.WriteLine("=== Währungsrechner (Frankfurter API) ===");
        Console.Write("Betrag: ");
        string? b = Console.ReadLine();
        if (!decimal.TryParse(b, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal betrag))
            return;
        Console.Write("Von (ISO, z. B. EUR): ");
        string? von = Console.ReadLine();
        Console.Write("Nach (ISO, z. B. USD): ");
        string? nach = Console.ReadLine();
        string v = (von ?? "").Trim().ToUpperInvariant();
        string n = (nach ?? "").Trim().ToUpperInvariant();
        if (v.Length != 3 || n.Length != 3)
            return;

        string url = string.Format(CultureInfo.InvariantCulture,
            "https://api.frankfurter.app/latest?from={0}&to={1}", v, n);
        using HttpClient client = CreateHttpClientForApi();
        HttpResponseMessage resp = client.GetAsync(url).GetAwaiter().GetResult();
        string json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        decimal kurs = ReadCurrencyRateFromJson(json, n);
        decimal ergebnis = betrag * kurs;
        Console.WriteLine(ergebnis.ToString("0.00", CultureInfo.InvariantCulture) + " " + n);
    }

    // Erstellt einen HttpClient mit Timeout.
    private static HttpClient CreateHttpClientForApi()
    {
        HttpClient c = new();
        c.Timeout = TimeSpan.FromSeconds(20);
        return c;
    }

    // Liest den Kurs aus dem JSON der API.
    private static decimal ReadCurrencyRateFromJson(string json, string targetIso)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
        if (!root.TryGetProperty("rates", out JsonElement rates))
            return 0m;
        if (rates.TryGetProperty(targetIso, out JsonElement wert))
            return wert.GetDecimal();

        return 0m;
    }

    // Baut einen QR-Code als ASCII-Ausgabe.
    private void RunQrCodeGenerator()
    {
        Console.WriteLine("=== QR-Code (Text) ===");
        Console.Write("Inhalt: ");
        string? t = Console.ReadLine();
        string text = t ?? "";
        if (text.Length == 0)
            return;
        using QRCodeGenerator gen = new();
        QRCodeData data = gen.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        AsciiQRCode ascii = new(data);
        string art = ascii.GetGraphic(1);
        Console.WriteLine(art);
    }

    // Berechnet die Tage zwischen zwei Datumswerten.
    private void RunDateCalculator()
    {
        Console.WriteLine("=== Datumrechner ===");
        Console.Write("Erstes Datum (dd.MM.yyyy): ");
        string? a = Console.ReadLine();
        Console.Write("Zweites Datum (dd.MM.yyyy): ");
        string? b = Console.ReadLine();
        string as_ = a?.Trim() ?? "";
        string bs = b?.Trim() ?? "";
        if (!DateTime.TryParseExact(as_, "dd.MM.yyyy", CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out DateTime d1))
            return;
        if (!DateTime.TryParseExact(bs, "dd.MM.yyyy", CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out DateTime d2))
            return;
        TimeSpan diff = d2.Date - d1.Date;
        int tage = Math.Abs(diff.Days);
        Console.WriteLine("Tage zwischen den Daten: " + tage);
    }

    private void RunWorldTimeTool()
    {
        Console.WriteLine("=== Weltzeit-Anzeige ===");
        DateTime utc = DateTime.UtcNow;
        ShowWorldTime("Wien", FindTimeZone("W. Europe Standard Time", "Europe/Vienna"), utc);
        ShowWorldTime("Tokyo", FindTimeZone("Tokyo Standard Time", "Asia/Tokyo"), utc);
        ShowWorldTime("New York", FindTimeZone("Eastern Standard Time", "America/New_York"), utc);
    }

    private static void ShowWorldTime(string name, TimeZoneInfo? zone, DateTime utcNow)
    {
        if (zone is null)
        {
            Console.WriteLine(name + ": Zeitzone nicht gefunden");
            return;
        }

        DateTime local = TimeZoneInfo.ConvertTimeFromUtc(utcNow, zone);
        Console.WriteLine(name + ": " + local.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
    }

    private static TimeZoneInfo? FindTimeZone(string windowsId, string ianaId)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(windowsId); }
        catch
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(ianaId); }
            catch { return null; }
        }
    }

    private void RunDiceSimulatorTool()
    {
        Console.WriteLine("=== Wuerfelsimulator ===");
        int seiten = PromptInt("Seiten (z. B. 6/20): ", 2, 1000);
        int anzahl = PromptInt("Anzahl Wuerfel: ", 1, 100);
        int summe = 0;
        for (int i = 1; i <= anzahl; i++)
        {
            int wurf = Random.Shared.Next(1, seiten + 1);
            Console.WriteLine("Wuerfel " + i + ": " + wurf);
            summe += wurf;
        }

        Console.WriteLine("Summe: " + summe);
    }

    private void RunQuizTool()
    {
        Console.WriteLine("=== Quiz ===");
        QuizEntry[] questions =
        [
            new QuizEntry("Wie viele Kontinente gibt es?", ["5", "6", "7"], 2),
            new QuizEntry("Welche Stadt liegt in Oesterreich?", ["Wien", "Berlin", "Prag"], 0),
            new QuizEntry("Was ist 12 / 3?", ["4", "3", "6"], 0),
            new QuizEntry("C# gehoert zu ...", [".NET", "JVM", "Node.js"], 0),
        ];

        int points = 0;
        for (int i = 0; i < questions.Length; i++)
        {
            Console.WriteLine();
            Console.WriteLine((i + 1) + ") " + questions[i].Question);
            for (int a = 0; a < questions[i].Answers.Length; a++)
                Console.WriteLine("   " + (a + 1) + ") " + questions[i].Answers[a]);
            int ans = PromptInt("Antwort: ", 1, questions[i].Answers.Length) - 1;
            if (ans == questions[i].CorrectIndex)
                points++;
        }

        Console.WriteLine();
        Console.WriteLine("Punkte: " + points + "/" + questions.Length);
    }

    private void RunExpenseTrackerTool()
    {
        Console.WriteLine("=== Ausgaben-Tracker ===");
        bool fertig = false;
        List<SimpleExpenseEntry> entries = [];
        while (!fertig)
        {
            Console.WriteLine("1) Neu  2) Uebersicht  3) Ende");
            Console.Write("> ");
            string? w = Console.ReadLine();
            string k = w?.Trim() ?? "";
            if (k == "3")
            {
                fertig = true;
                continue;
            }

            if (k == "1")
            {
                Console.Write("Kategorie: ");
                string cat = (Console.ReadLine() ?? "").Trim();
                if (cat.Length == 0)
                    cat = "Allgemein";
                decimal amount = PromptDecimal("Betrag: ");
                if (amount < 0)
                {
                    Console.WriteLine("Nur positive Werte.");
                    continue;
                }
                SimpleExpenseEntry e = new SimpleExpenseEntry();
                e.Category = cat;
                e.Amount = amount;
                entries.Add(e);
            }
            else if (k == "2")
            {
                if (entries.Count == 0)
                {
                    Console.WriteLine("(keine Eintraege)");
                    continue;
                }
                PrintExpenseOverview(entries);
            }
        }
    }

    // Zeigt die Ausgaben gesamt und pro Kategorie ohne LINQ.
    private static void PrintExpenseOverview(List<SimpleExpenseEntry> entries)
    {
        decimal total = 0m;
        for (int i = 0; i < entries.Count; i++)
        {
            total += entries[i].Amount;
        }

        Console.WriteLine("Gesamt: " + total.ToString("0.00", CultureInfo.InvariantCulture));

        List<string> cats = new List<string>();
        List<decimal> sums = new List<decimal>();
        for (int i = 0; i < entries.Count; i++)
        {
            SimpleExpenseEntry e = entries[i];
            int idx = FindCategoryIndex(cats, e.Category);
            if (idx == -1)
            {
                cats.Add(e.Category);
                sums.Add(e.Amount);
            }
            else
            {
                sums[idx] = sums[idx] + e.Amount;
            }
        }

        for (int i = 0; i < cats.Count; i++)
        {
            Console.WriteLine(cats[i] + ": " + sums[i].ToString("0.00", CultureInfo.InvariantCulture));
        }
    }

    // Sucht Kategorie ohne Groß-/Kleinschreibung.
    private static int FindCategoryIndex(List<string> cats, string value)
    {
        for (int i = 0; i < cats.Count; i++)
        {
            if (string.Equals(cats[i], value, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private static int PromptInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            string? t = Console.ReadLine();
            if (int.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) && v >= min && v <= max)
                return v;
            Console.WriteLine("Bitte Zahl zwischen " + min + " und " + max + ".");
        }
    }

    private static decimal PromptDecimal(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string raw = (Console.ReadLine() ?? "").Trim().Replace(',', '.');
            if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
                return value;
            Console.WriteLine("Bitte gueltige Zahl eingeben.");
        }
    }

    private sealed class QuizEntry
    {
        public QuizEntry(string question, string[] answers, int correctIndex)
        {
            Question = question;
            Answers = answers;
            CorrectIndex = correctIndex;
        }

        public string Question { get; private set; }
        public string[] Answers { get; private set; }
        public int CorrectIndex { get; private set; }
    }

    private sealed class SimpleExpenseEntry
    {
        public string Category { get; set; } = "Allgemein";
        public decimal Amount { get; set; }
    }
}

// Was macht diese Datei?
// - Enthält mehrere Tools: Umrechner, Taschenrechner, Caesar, Textanalyse,
//   Währungsrechner, QR-Code und Datumrechner.
// - Jede Methode ist ein einzelnes Werkzeug im Admin-Menü.
