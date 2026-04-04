using System.Data;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using QRCoder;

namespace AdminApp;

public sealed partial class AdminPortal
{
    private void EinheitenrechnerAusfuehren()
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
                EinheitKmNachM(v);
            else if (k == "2")
                EinheitMNachKm(v);
            else if (k == "3")
                EinheitKgNachLb(v);
            else if (k == "4")
                EinheitLbNachKg(v);
            else if (k == "5")
                EinheitCelsiusNachFahrenheit(v);
            else if (k == "6")
                EinheitFahrenheitNachCelsius(v);
        }
    }

    private static void EinheitKmNachM(double km)
    {
        Console.WriteLine((km * 1000.0).ToString("0.###", CultureInfo.InvariantCulture) + " m");
    }

    private static void EinheitMNachKm(double m)
    {
        Console.WriteLine((m / 1000.0).ToString("0.######", CultureInfo.InvariantCulture) + " km");
    }

    private static void EinheitKgNachLb(double kg)
    {
        Console.WriteLine((kg * 2.2046226218).ToString("0.###", CultureInfo.InvariantCulture) + " lb");
    }

    private static void EinheitLbNachKg(double lb)
    {
        Console.WriteLine((lb / 2.2046226218).ToString("0.###", CultureInfo.InvariantCulture) + " kg");
    }

    private static void EinheitCelsiusNachFahrenheit(double c)
    {
        double f = c * 9.0 / 5.0 + 32.0;
        Console.WriteLine(f.ToString("0.###", CultureInfo.InvariantCulture) + " °F");
    }

    private static void EinheitFahrenheitNachCelsius(double f)
    {
        double c = (f - 32.0) * 5.0 / 9.0;
        Console.WriteLine(c.ToString("0.###", CultureInfo.InvariantCulture) + " °C");
    }

    private void TaschenrechnerMitVerlaufAusfuehren()
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
                TaschenrechnerVerlaufDrucken(verlauf);
                continue;
            }

            if (ausdruck.Length == 0)
                continue;

            string? ergebnisText = TaschenrechnerAusdruckAuswerten(ausdruck);
            if (ergebnisText == null)
            {
                Console.WriteLine("Ungültiger Ausdruck.");
                continue;
            }

            Console.WriteLine("= " + ergebnisText);
            verlauf.Add(ausdruck + " = " + ergebnisText);
        }
    }

    private static void TaschenrechnerVerlaufDrucken(List<string> verlauf)
    {
        if (verlauf.Count == 0)
        {
            Console.WriteLine("(leer)");
            return;
        }

        foreach (string z in verlauf)
            Console.WriteLine(z);
    }

    private static string? TaschenrechnerAusdruckAuswerten(string ausdruck)
    {
        string norm = ausdruck.Replace(',', '.');
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

    private void CaesarWerkzeugAusfuehren()
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
        string outText = CaesarTextTransformieren(text, shift, enc);
        Console.WriteLine(outText);
    }

    private static string CaesarTextTransformieren(string text, int shift, bool verschluesseln)
    {
        int s = verschluesseln ? shift : 26 - shift;
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

    private void TextAnalyzerAusfuehren()
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
        int zeichenOhneLeer = voll.Count(c => !char.IsWhiteSpace(c));
        int saetze = TextAnalyzerSaetzeZaehlen(voll);
        Console.WriteLine("Wörter: " + woerter);
        Console.WriteLine("Zeichen (mit Leer): " + zeichenMitLeer);
        Console.WriteLine("Zeichen (ohne Leer): " + zeichenOhneLeer);
        Console.WriteLine("Sätze: " + saetze);
    }

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

    private void WaehrungsrechnerAusfuehren()
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
        using HttpClient client = HttpClientFuerApiErzeugen();
        HttpResponseMessage resp = client.GetAsync(url).GetAwaiter().GetResult();
        string json = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        decimal kurs = WaehrungsJsonKursLesen(json, n);
        decimal ergebnis = betrag * kurs;
        Console.WriteLine(ergebnis.ToString("0.00", CultureInfo.InvariantCulture) + " " + n);
    }

    private static HttpClient HttpClientFuerApiErzeugen()
    {
        HttpClient c = new();
        c.Timeout = TimeSpan.FromSeconds(20);
        return c;
    }

    private static decimal WaehrungsJsonKursLesen(string json, string zielIso)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
        if (!root.TryGetProperty("rates", out JsonElement rates))
            return 0m;
        if (rates.TryGetProperty(zielIso, out JsonElement wert))
            return wert.GetDecimal();

        return 0m;
    }

    private void QrCodeGeneratorAusfuehren()
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

    private void DatumrechnerAusfuehren()
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
}
