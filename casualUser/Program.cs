using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CasualUserApp;

public sealed class CasualPortal
{
    private readonly List<string> notes = new List<string>();
    private readonly List<TodoItem> todos = new List<TodoItem>();
    private readonly List<ExpenseEntry> expenses = new List<ExpenseEntry>();
    private readonly string settingsPath = Path.Combine(AppContext.BaseDirectory, "casual.settings.json");

    private bool soundOn = true;
    private ConsoleColor accentColor = ConsoleColor.Cyan;

    private static readonly string[] menuItems = new string[]
    {
        "Notiz schreiben",
        "Notiz-Liste (Mehrere Notizen)",
        "To-Do Liste",
        "Taschenrechner",
        "Einheitenrechner",
        "Datumrechner (Tage zwischen 2 Daten)",
        "Timer / Countdown",
        "Passwort-Generator",
        "Text-Tools",
        "BMI-Rechner",
        "Zufallszahl-Generator",
        "Wuerfelsimulator",
        "Morse-Uebersetzer",
        "Caesar-Verschluesselung",
        "Weltzeit-Anzeige",
        "Quiz",
        "Ausgaben-Tracker",
        "Einstellungen",
        "Beenden"
    };

    private static readonly Dictionary<char, string> morseMap = new Dictionary<char, string>
    {
        { 'A', ".-" }, { 'B', "-..." }, { 'C', "-.-." }, { 'D', "-.." }, { 'E', "." }, { 'F', "..-." },
        { 'G', "--." }, { 'H', "...." }, { 'I', ".." }, { 'J', ".---" }, { 'K', "-.-" }, { 'L', ".-.." },
        { 'M', "--" }, { 'N', "-." }, { 'O', "---" }, { 'P', ".--." }, { 'Q', "--.-" }, { 'R', ".-." },
        { 'S', "..." }, { 'T', "-" }, { 'U', "..-" }, { 'V', "...-" }, { 'W', ".--" }, { 'X', "-..-" },
        { 'Y', "-.--" }, { 'Z', "--.." }, { '0', "-----" }, { '1', ".----" }, { '2', "..---" },
        { '3', "...--" }, { '4', "....-" }, { '5', "....." }, { '6', "-...." }, { '7', "--..." },
        { '8', "---.." }, { '9', "----." }
    };

    // Startet das Portal von Anfang bis Ende.
    public void Run()
    {
        PrepareConsole();
        LoadSettings();
        RunMainMenu();
        SaveSettings();
    }

    // Setzt die Konsole in einen sauberen Startzustand.
    private static void PrepareConsole()
    {
        if (Console.IsOutputRedirected)
        {
            return;
        }

        Console.OutputEncoding = new UTF8Encoding(false);
        Console.Clear();
        Console.ResetColor();
        Console.CursorVisible = true;
    }

    // Zeigt das Hauptmenue und fuehrt gewaehlte Aktionen aus.
    private void RunMainMenu()
    {
        bool keepRunning = true;

        while (keepRunning)
        {
            int index;
            if (Console.IsOutputRedirected)
            {
                index = SelectMainMenuTextOnly();
            }
            else
            {
                index = SelectMainMenuWithArrows();
            }

            keepRunning = ExecuteMainAction(index);

            if (keepRunning)
            {
                WaitForEnter();
            }
        }
    }

    // Gibt im Textmodus ein nummeriertes Menue zur Auswahl aus.
    private static int SelectMainMenuTextOnly()
    {
        Console.Clear();
        Console.WriteLine("=== Casual Portal ===");
        for (int i = 0; i < menuItems.Length; i++)
        {
            Console.WriteLine((i + 1) + ") " + menuItems[i]);
        }

        int selected = ReadInt("Bitte Menuepunkt waehlen: ", 1, menuItems.Length);
        return selected - 1;
    }

    // Gibt im interaktiven Modus das Menue mit Pfeiltasten aus.
    private int SelectMainMenuWithArrows()
    {
        int index = 0;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Casual Portal ===");
            Console.WriteLine("Mit Pfeiltasten waehlen, Enter bestaetigt.");
            Console.WriteLine();

            for (int i = 0; i < menuItems.Length; i++)
            {
                if (i == index)
                {
                    Console.ForegroundColor = accentColor;
                    Console.Write("> ");
                }
                else
                {
                    Console.Write("  ");
                }

                Console.WriteLine(menuItems[i]);
                Console.ResetColor();
            }

            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.UpArrow)
            {
                if (index == 0)
                {
                    index = menuItems.Length - 1;
                }
                else
                {
                    index = index - 1;
                }

                PlayNavigationSound();
            }
            else if (key.Key == ConsoleKey.DownArrow)
            {
                if (index == menuItems.Length - 1)
                {
                    index = 0;
                }
                else
                {
                    index = index + 1;
                }

                PlayNavigationSound();
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                return index;
            }
        }
    }

    // Führt die gewählte Hauptaktion aus und sagt, ob das Programm weiterlaufen soll.
    private bool ExecuteMainAction(int index)
    {
        if (index == 0) RunNoteWriter();
        else if (index == 1) RunNoteList();
        else if (index == 2) RunTodoList();
        else if (index == 3) RunCalculator();
        else if (index == 4) RunUnitConverter();
        else if (index == 5) RunDateCalculator();
        else if (index == 6) RunCountdown();
        else if (index == 7) RunPasswordGenerator();
        else if (index == 8) RunTextTools();
        else if (index == 9) RunBmiCalculator();
        else if (index == 10) RunRandomNumber();
        else if (index == 11) RunDiceSimulator();
        else if (index == 12) RunMorseTranslator();
        else if (index == 13) RunCaesarTool();
        else if (index == 14) RunWorldTime();
        else if (index == 15) RunQuiz();
        else if (index == 16) RunExpenseTracker();
        else if (index == 17) RunSettingsMenu();
        else if (index == 18) return false;

        return true;
    }

    // Schreibt eine einzelne neue Notiz.
    private void RunNoteWriter()
    {
        Console.Clear();
        Console.WriteLine("=== Notiz schreiben ===");
        Console.Write("Titel: ");
        string title = (Console.ReadLine() ?? string.Empty).Trim();
        Console.Write("Inhalt: ");
        string text = (Console.ReadLine() ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            Console.WriteLine("Leere Notiz wurde nicht gespeichert.");
            return;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            title = "Notiz";
        }

        string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm") + " | " + title + ": " + text;
        notes.Add(line);
        Console.WriteLine("Notiz gespeichert.");
    }

    // Zeigt die Notizliste und bietet Zusatzaktionen an.
    private void RunNoteList()
    {
        Console.Clear();
        Console.WriteLine("=== Notiz-Liste ===");

        if (notes.Count == 0)
        {
            Console.WriteLine("Noch keine Notizen vorhanden.");
            return;
        }

        for (int i = 0; i < notes.Count; i++)
        {
            Console.WriteLine((i + 1) + ") " + notes[i]);
        }

        Console.WriteLine();
        Console.WriteLine("1) Notiz loeschen");
        Console.WriteLine("2) Notizen suchen");
        Console.WriteLine("3) Notizen exportieren (.txt)");
        Console.WriteLine("4) Zurueck");

        int choice = ReadInt("Auswahl: ", 1, 4);
        if (choice == 1)
        {
            int nr = ReadInt("Nummer zum Loeschen: ", 1, notes.Count);
            notes.RemoveAt(nr - 1);
            Console.WriteLine("Notiz geloescht.");
        }
        else if (choice == 2)
        {
            RunNoteSearch();
        }
        else if (choice == 3)
        {
            ExportNotesToTxt();
        }
    }

    // Sucht Notizen mit einem Stichwort.
    private void RunNoteSearch()
    {
        Console.Write("Stichwort: ");
        string key = (Console.ReadLine() ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        List<string> found = new List<string>();
        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i].Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                found.Add(notes[i]);
            }
        }

        if (found.Count == 0)
        {
            Console.WriteLine("Keine Treffer.");
            return;
        }

        Console.WriteLine("Treffer (" + found.Count + "):");
        for (int i = 0; i < found.Count; i++)
        {
            Console.WriteLine("- " + found[i]);
        }
    }

    // Exportiert die Notizen als Textdatei.
    private void ExportNotesToTxt()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "notizen_export_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");

        if (notes.Count == 0)
        {
            File.WriteAllText(path, "Keine Notizen vorhanden.", Encoding.UTF8);
            Console.WriteLine("Leere Exportdatei erstellt: " + path);
            return;
        }

        File.WriteAllLines(path, notes, Encoding.UTF8);
        Console.WriteLine("Exportiert nach: " + path);
    }

    // Steuert die komplette To-Do-Verwaltung.
    private void RunTodoList()
    {
        bool inMenu = true;
        while (inMenu)
        {
            Console.Clear();
            Console.WriteLine("=== To-Do Liste ===");
            PrintTodoList();

            Console.WriteLine();
            Console.WriteLine("1) Aufgabe hinzufuegen");
            Console.WriteLine("2) Aufgabe erledigt markieren");
            Console.WriteLine("3) Aufgabe loeschen");
            Console.WriteLine("4) Prioritaet aendern");
            Console.WriteLine("5) Zurueck");

            int choice = ReadInt("Auswahl: ", 1, 5);
            if (choice == 1) AddTodo();
            else if (choice == 2) ToggleTodoDone();
            else if (choice == 3) DeleteTodo();
            else if (choice == 4) ChangeTodoPriority();
            else if (choice == 5) inMenu = false;
        }
    }

    // Zeigt alle aktuellen To-Do-Eintraege an.
    private void PrintTodoList()
    {
        if (todos.Count == 0)
        {
            Console.WriteLine("(leer)");
            return;
        }

        for (int i = 0; i < todos.Count; i++)
        {
            TodoItem t = todos[i];
            string mark = t.Done ? "[x]" : "[ ]";
            Console.WriteLine((i + 1) + ") " + mark + " (" + t.Priority + ") " + t.Text);
        }
    }

    // Fuegt eine neue Aufgabe zur Liste hinzu.
    private void AddTodo()
    {
        Console.Write("Aufgabe: ");
        string text = (Console.ReadLine() ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        TodoPriority p = ReadPriority();
        TodoItem item = new TodoItem();
        item.Text = text;
        item.Done = false;
        item.Priority = p;
        todos.Add(item);
    }

    // Schaltet den Erledigt-Status einer Aufgabe um.
    private void ToggleTodoDone()
    {
        if (todos.Count == 0)
        {
            return;
        }

        int idx = ReadInt("Nummer: ", 1, todos.Count) - 1;
        TodoItem item = todos[idx];
        item.Done = !item.Done;
        todos[idx] = item;
    }

    // Loescht eine Aufgabe aus der Liste.
    private void DeleteTodo()
    {
        if (todos.Count == 0)
        {
            return;
        }

        int idx = ReadInt("Nummer: ", 1, todos.Count) - 1;
        todos.RemoveAt(idx);
    }

    // Aendert die Prioritaet einer Aufgabe.
    private void ChangeTodoPriority()
    {
        if (todos.Count == 0)
        {
            return;
        }

        int idx = ReadInt("Nummer: ", 1, todos.Count) - 1;
        TodoItem item = todos[idx];
        item.Priority = ReadPriority();
        todos[idx] = item;
    }

    // Liest die Prioritaet fuer eine Aufgabe ein.
    private static TodoPriority ReadPriority()
    {
        Console.WriteLine("Prioritaet: 1) Niedrig  2) Mittel  3) Hoch");
        int p = ReadInt("Auswahl: ", 1, 3);

        if (p == 1) return TodoPriority.Low;
        if (p == 2) return TodoPriority.Medium;
        return TodoPriority.High;
    }

    // Rechnet zwei Zahlen mit einem einfachen Operator.
    private static void RunCalculator()
    {
        Console.Clear();
        Console.WriteLine("=== Taschenrechner ===");
        double a = ReadDouble("Zahl 1: ");
        Console.Write("Operator (+ - * /): ");
        string op = (Console.ReadLine() ?? string.Empty).Trim();
        double b = ReadDouble("Zahl 2: ");

        double result = 0;
        bool ok = true;

        if (op == "+") result = a + b;
        else if (op == "-") result = a - b;
        else if (op == "*") result = a * b;
        else if (op == "/")
        {
            if (Math.Abs(b) < double.Epsilon)
            {
                Console.WriteLine("Division durch 0 ist nicht erlaubt.");
                ok = false;
            }
            else
            {
                result = a / b;
            }
        }
        else
        {
            Console.WriteLine("Unbekannter Operator.");
            ok = false;
        }

        if (ok)
        {
            Console.WriteLine("Ergebnis: " + result);
        }
    }

    // Konvertiert Werte zwischen ausgewaehlten Einheiten.
    private static void RunUnitConverter()
    {
        Console.Clear();
        Console.WriteLine("=== Einheitenrechner ===");
        Console.WriteLine("1) Celsius -> Fahrenheit");
        Console.WriteLine("2) Fahrenheit -> Celsius");
        Console.WriteLine("3) Kilometer -> Meilen");
        Console.WriteLine("4) Meilen -> Kilometer");

        int choice = ReadInt("Auswahl: ", 1, 4);
        if (choice == 1)
        {
            double c = ReadDouble("Celsius: ");
            Console.WriteLine("Fahrenheit: " + ((c * 9 / 5) + 32).ToString("F2", CultureInfo.InvariantCulture));
        }
        else if (choice == 2)
        {
            double f = ReadDouble("Fahrenheit: ");
            Console.WriteLine("Celsius: " + ((f - 32) * 5 / 9).ToString("F2", CultureInfo.InvariantCulture));
        }
        else if (choice == 3)
        {
            double km = ReadDouble("Kilometer: ");
            Console.WriteLine("Meilen: " + (km * 0.621371).ToString("F3", CultureInfo.InvariantCulture));
        }
        else
        {
            double mi = ReadDouble("Meilen: ");
            Console.WriteLine("Kilometer: " + (mi / 0.621371).ToString("F3", CultureInfo.InvariantCulture));
        }
    }

    // Berechnet die Anzahl Tage zwischen zwei Daten.
    private static void RunDateCalculator()
    {
        Console.Clear();
        Console.WriteLine("=== Datumrechner ===");
        DateTime d1 = ReadDate("Erstes Datum (YYYY-MM-DD): ");
        DateTime d2 = ReadDate("Zweites Datum (YYYY-MM-DD): ");
        int days = Math.Abs((d2.Date - d1.Date).Days);
        Console.WriteLine("Differenz: " + days + " Tag(e)");
    }

    // Startet einen einfachen Countdown in Sekunden.
    private static void RunCountdown()
    {
        Console.Clear();
        Console.WriteLine("=== Timer / Countdown ===");
        int hours = ReadInt("Stunden: ", 0, 23);
        int mins = ReadInt("Minuten: ", 0, 59);
        int secs = (hours * 3600) + (mins * 60);

        if (secs <= 0)
        {
            Console.WriteLine("Bitte mindestens 1 Minute oder Stunde eingeben.");
            return;
        }

        Console.WriteLine("Countdown startet...");
        for (int left = secs; left >= 0; left--)
        {
            TimeSpan t = TimeSpan.FromSeconds(left);
            Console.Write("\rVerbleibend: " + t.ToString(@"hh\:mm\:ss") + " ");
            Thread.Sleep(1000);
        }

        Console.WriteLine();
        Console.WriteLine("Zeit abgelaufen.");
        if (OperatingSystem.IsWindows())
        {
            Console.Beep(1000, 250);
        }
    }

    // Erzeugt ein zufaelliges Passwort mit sicherer Zufallsquelle.
    private static void RunPasswordGenerator()
    {
        Console.Clear();
        Console.WriteLine("=== Passwort-Generator ===");
        int len = ReadInt("Laenge (6-64): ", 6, 64);
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+";

        byte[] bytes = new byte[len];
        RandomNumberGenerator.Fill(bytes);

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < len; i++)
        {
            int idx = bytes[i] % chars.Length;
            sb.Append(chars[idx]);
        }

        Console.WriteLine("Passwort: " + sb);
    }

    // Bietet mehrere einfache Textfunktionen an.
    private static void RunTextTools()
    {
        Console.Clear();
        Console.WriteLine("=== Text-Tools ===");
        Console.Write("Text eingeben: ");
        string input = Console.ReadLine() ?? string.Empty;

        Console.WriteLine("1) GROSS");
        Console.WriteLine("2) klein");
        Console.WriteLine("3) Umkehren");
        Console.WriteLine("4) Zeichen/Woerter zaehlen");
        Console.WriteLine("5) Duplikate entfernen (Zeilen)");
        Console.WriteLine("6) Zeichen ohne Leerzeichen");
        Console.WriteLine("7) Palindrom-Check");

        int choice = ReadInt("Auswahl: ", 1, 7);
        if (choice == 1) Console.WriteLine(input.ToUpperInvariant());
        else if (choice == 2) Console.WriteLine(input.ToLowerInvariant());
        else if (choice == 3) Console.WriteLine(ReverseText(input));
        else if (choice == 4) PrintCharAndWordCount(input);
        else if (choice == 5) PrintLinesWithoutDuplicates(input);
        else if (choice == 6) PrintCountWithoutSpaces(input);
        else if (choice == 7) PrintPalindromeCheck(input);
    }

    // Dreht einen Text Zeichen fuer Zeichen um.
    private static string ReverseText(string text)
    {
        char[] arr = text.ToCharArray();
        Array.Reverse(arr);
        return new string(arr);
    }

    // Zaehlt Zeichen und Woerter in einem Text.
    private static void PrintCharAndWordCount(string text)
    {
        int chars = text.Length;
        int words = CountWords(text);
        Console.WriteLine("Zeichen: " + chars + ", Woerter: " + words);
    }

    // Zaehlt Woerter ohne Split/LINQ, damit die Logik sichtbar bleibt.
    private static int CountWords(string text)
    {
        int count = 0;
        bool inWord = false;

        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsWhiteSpace(text[i]))
            {
                inWord = false;
            }
            else
            {
                if (!inWord)
                {
                    count++;
                }

                inWord = true;
            }
        }

        return count;
    }

    // Entfernt doppelte Zeilen und gibt nur den ersten Treffer aus.
    private static void PrintLinesWithoutDuplicates(string text)
    {
        string[] lines = text.Replace("\r\n", "\n").Split('\n');
        List<string> uniq = new List<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            bool exists = false;

            // Wir vergleichen jede neue Zeile mit den schon gespeicherten Zeilen.
            for (int j = 0; j < uniq.Count; j++)
            {
                if (uniq[j] == line)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                uniq.Add(line);
            }
        }

        Console.WriteLine(string.Join(Environment.NewLine, uniq));
    }

    // Zaehlt alle Zeichen, die keine Leerzeichen sind.
    private static void PrintCountWithoutSpaces(string text)
    {
        int count = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (!char.IsWhiteSpace(text[i]))
            {
                count++;
            }
        }

        Console.WriteLine("Zeichen ohne Leerzeichen: " + count);
    }

    // Prueft, ob ein Text vorwaerts und rueckwaerts gleich ist.
    private static void PrintPalindromeCheck(string text)
    {
        string clean = BuildNormalizedText(text);
        bool ok = IsPalindrome(clean);
        if (ok) Console.WriteLine("Ist ein Palindrom.");
        else Console.WriteLine("Ist kein Palindrom.");
    }

    // Entfernt Sonderzeichen und macht den Text fuer den Vergleich klein.
    private static string BuildNormalizedText(string text)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }

        return sb.ToString();
    }

    // Vergleicht Zeichenpaare von aussen nach innen.
    private static bool IsPalindrome(string text)
    {
        if (text.Length == 0)
        {
            return false;
        }

        int left = 0;
        int right = text.Length - 1;
        while (left < right)
        {
            if (text[left] != text[right])
            {
                return false;
            }

            left++;
            right--;
        }

        return true;
    }

    // Berechnet den BMI und ordnet ihn in eine Kategorie ein.
    private static void RunBmiCalculator()
    {
        Console.Clear();
        Console.WriteLine("=== BMI-Rechner ===");
        double kg = ReadDouble("Gewicht (kg): ");
        double cm = ReadDouble("Groesse (cm): ");
        double m = cm / 100.0;

        if (m <= 0)
        {
            Console.WriteLine("Ungueltige Groesse.");
            return;
        }

        double bmi = kg / (m * m);
        Console.WriteLine("BMI: " + bmi.ToString("F1", CultureInfo.InvariantCulture));
        Console.WriteLine(GetBmiCategory(bmi));
    }

    // Liefert die BMI-Kategorie als Text.
    private static string GetBmiCategory(double bmi)
    {
        if (bmi < 18.5) return "Kategorie: Untergewicht";
        if (bmi < 25.0) return "Kategorie: Normalgewicht";
        if (bmi < 30.0) return "Kategorie: Uebergewicht";
        return "Kategorie: Adipositas";
    }

    // Erzeugt eine Zufallszahl in einem Bereich.
    private static void RunRandomNumber()
    {
        Console.Clear();
        Console.WriteLine("=== Zufallszahl-Generator ===");
        int min = ReadInt("Minimum: ", int.MinValue + 1, int.MaxValue - 1);
        int max = ReadInt("Maximum: ", min + 1, int.MaxValue);
        int num = Random.Shared.Next(min, max + 1);
        Console.WriteLine("Zufallszahl: " + num);
    }

    // Simuliert mehrere Wuerfelwuerfe und summiert sie.
    private static void RunDiceSimulator()
    {
        Console.Clear();
        Console.WriteLine("=== Wuerfelsimulator ===");
        int sides = ReadInt("Seiten (z. B. 6, 20): ", 2, 1000);
        int count = ReadInt("Anzahl Wuerfel: ", 1, 100);

        int sum = 0;
        for (int i = 1; i <= count; i++)
        {
            int roll = Random.Shared.Next(1, sides + 1);
            Console.WriteLine("Wuerfel " + i + ": " + roll);
            sum += roll;
        }

        Console.WriteLine("Summe: " + sum);
    }

    // Uebersetzt zwischen normalem Text und Morse-Code.
    private static void RunMorseTranslator()
    {
        Console.Clear();
        Console.WriteLine("=== Morse-Uebersetzer ===");
        Console.WriteLine("1) Text -> Morse");
        Console.WriteLine("2) Morse -> Text");
        int choice = ReadInt("Auswahl: ", 1, 2);

        if (choice == 1)
        {
            Console.Write("Text: ");
            string text = Console.ReadLine() ?? string.Empty;
            string morse = ConvertTextToMorse(text);
            Console.WriteLine(morse);
        }
        else
        {
            Console.Write("Morse (mit Leerzeichen, / fuer Worttrennung): ");
            string text = Console.ReadLine() ?? string.Empty;
            string normal = ConvertMorseToText(text);
            Console.WriteLine(normal);
        }
    }

    // Wandelt normalen Text in Morse-Zeichen um.
    private static string ConvertTextToMorse(string text)
    {
        List<string> parts = new List<string>();
        string up = text.ToUpperInvariant();

        for (int i = 0; i < up.Length; i++)
        {
            char c = up[i];
            if (c == ' ')
            {
                parts.Add("/");
            }
            else
            {
                string? morse;
                if (morseMap.TryGetValue(c, out morse))
                {
                    parts.Add(morse);
                }
            }
        }

        return string.Join(" ", parts);
    }

    // Wandelt Morse-Zeichen wieder in normalen Text um.
    private static string ConvertMorseToText(string text)
    {
        string[] parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i];
            if (part == "/")
            {
                sb.Append(' ');
            }
            else
            {
                char c = FindCharByMorse(part);
                if (c != '\0')
                {
                    sb.Append(c);
                }
            }
        }

        return sb.ToString();
    }

    // Sucht den passenden Buchstaben zu einem Morse-Code.
    private static char FindCharByMorse(string morse)
    {
        foreach (KeyValuePair<char, string> pair in morseMap)
        {
            if (pair.Value == morse)
            {
                return pair.Key;
            }
        }

        return '\0';
    }

    // Startet die Caesar-Ver- oder Entschluesselung.
    private static void RunCaesarTool()
    {
        Console.Clear();
        Console.WriteLine("=== Caesar-Verschluesselung ===");
        Console.Write("1=verschluesseln 2=entschluesseln: ");
        string mode = (Console.ReadLine() ?? string.Empty).Trim();
        int shift = ReadInt("Verschiebung (1-25): ", 1, 25);
        Console.Write("Text: ");
        string text = Console.ReadLine() ?? string.Empty;

        bool encrypt = mode == "1";
        string result = CaesarTransform(text, shift, encrypt);
        Console.WriteLine(result);
    }

    // Verschiebt jeden Buchstaben im Alphabet um eine feste Anzahl.
    private static string CaesarTransform(string text, int shift, bool encrypt)
    {
        int realShift;
        if (encrypt)
        {
            realShift = shift;
        }
        else
        {
            realShift = 26 - shift;
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c >= 'a' && c <= 'z')
            {
                int pos = c - 'a';
                int newPos = (pos + realShift) % 26;
                sb.Append((char)('a' + newPos));
            }
            else if (c >= 'A' && c <= 'Z')
            {
                int pos = c - 'A';
                int newPos = (pos + realShift) % 26;
                sb.Append((char)('A' + newPos));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    // Zeigt die aktuelle Zeit in mehreren Staedten an.
    private static void RunWorldTime()
    {
        Console.Clear();
        Console.WriteLine("=== Weltzeit-Anzeige ===");
        DateTime utc = DateTime.UtcNow;
        WriteWorldClock("Wien", FindTimeZone("W. Europe Standard Time", "Europe/Vienna"), utc);
        WriteWorldClock("Tokyo", FindTimeZone("Tokyo Standard Time", "Asia/Tokyo"), utc);
        WriteWorldClock("New York", FindTimeZone("Eastern Standard Time", "America/New_York"), utc);
    }

    // Druckt eine einzelne Uhrzeit fuer einen Ort.
    private static void WriteWorldClock(string label, TimeZoneInfo? zone, DateTime utc)
    {
        if (zone == null)
        {
            Console.WriteLine(label + ": Zeitzone nicht gefunden");
            return;
        }

        DateTime local = TimeZoneInfo.ConvertTimeFromUtc(utc, zone);
        Console.WriteLine(label + ": " + local.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    // Findet eine Zeitzone mit Windows-ID oder alternativ IANA-ID.
    private static TimeZoneInfo? FindTimeZone(string windowsId, string ianaId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(windowsId);
        }
        catch
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(ianaId);
            }
            catch
            {
                return null;
            }
        }
    }

    // Startet ein kleines Multiple-Choice-Quiz.
    private static void RunQuiz()
    {
        Console.Clear();
        Console.WriteLine("=== Quiz ===");
        List<QuizQuestion> list = BuildQuizQuestions();

        int points = 0;
        for (int i = 0; i < list.Count; i++)
        {
            QuizQuestion q = list[i];
            Console.WriteLine();
            Console.WriteLine((i + 1) + ") " + q.Question);
            for (int a = 0; a < q.Answers.Length; a++)
            {
                Console.WriteLine("   " + (a + 1) + ") " + q.Answers[a]);
            }

            int ans = ReadInt("Antwort: ", 1, q.Answers.Length) - 1;
            if (ans == q.CorrectIndex)
            {
                points++;
            }
        }

        Console.WriteLine();
        Console.WriteLine("Punktestand: " + points + "/" + list.Count);
    }

    // Baut die Fragenliste fuer das Quiz auf.
    private static List<QuizQuestion> BuildQuizQuestions()
    {
        List<QuizQuestion> list = new List<QuizQuestion>();
        list.Add(new QuizQuestion("Wie viele Kontinente gibt es auf der Erde?", new string[] { "5", "6", "7" }, 2));
        list.Add(new QuizQuestion("Welche Sprache laeuft auf der .NET Runtime?", new string[] { "C#", "Python", "Go" }, 0));
        list.Add(new QuizQuestion("Was ist 9 * 7?", new string[] { "63", "56", "72" }, 0));
        list.Add(new QuizQuestion("Welche Stadt ist in Oesterreich?", new string[] { "Wien", "Bern", "Hamburg" }, 0));
        return list;
    }

    // Verwaltet das Menue fuer Ausgaben-Eintraege.
    private void RunExpenseTracker()
    {
        bool running = true;
        while (running)
        {
            Console.Clear();
            Console.WriteLine("=== Ausgaben-Tracker ===");
            Console.WriteLine("1) Ausgabe hinzufuegen");
            Console.WriteLine("2) Uebersicht anzeigen");
            Console.WriteLine("3) Zurueck");

            int choice = ReadInt("Auswahl: ", 1, 3);
            if (choice == 1)
            {
                AddExpense();
            }
            else if (choice == 2)
            {
                ShowExpenseSummary();
                WaitForEnter();
            }
            else
            {
                running = false;
            }
        }
    }

    // Fuegt einen neuen Ausgaben-Eintrag hinzu.
    private void AddExpense()
    {
        Console.Write("Kategorie: ");
        string cat = (Console.ReadLine() ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(cat))
        {
            cat = "Allgemein";
        }

        double amount = ReadDouble("Betrag: ");
        if (amount < 0)
        {
            Console.WriteLine("Nur positive Werte.");
            WaitForEnter();
            return;
        }

        ExpenseEntry e = new ExpenseEntry();
        e.Category = cat;
        e.Amount = amount;
        expenses.Add(e);
    }

    // Zeigt Gesamt- und Kategorien-Summen ohne LINQ.
    private void ShowExpenseSummary()
    {
        Console.Clear();
        Console.WriteLine("=== Ausgaben-Uebersicht ===");

        if (expenses.Count == 0)
        {
            Console.WriteLine("(keine Eintraege)");
            return;
        }

        double total = 0;
        for (int i = 0; i < expenses.Count; i++)
        {
            total += expenses[i].Amount;
        }

        Console.WriteLine("Gesamt: " + total.ToString("F2", CultureInfo.InvariantCulture));
        Console.WriteLine();

        List<string> cats = new List<string>();
        List<double> sums = new List<double>();

        // Wir bauen eigene Gruppen per Schleife auf, damit die Logik klar sichtbar bleibt.
        for (int i = 0; i < expenses.Count; i++)
        {
            string cat = expenses[i].Category;
            int idx = IndexOfIgnoreCase(cats, cat);
            if (idx == -1)
            {
                cats.Add(cat);
                sums.Add(expenses[i].Amount);
            }
            else
            {
                sums[idx] = sums[idx] + expenses[i].Amount;
            }
        }

        for (int i = 0; i < cats.Count; i++)
        {
            Console.WriteLine(cats[i] + ": " + sums[i].ToString("F2", CultureInfo.InvariantCulture));
        }
    }

    // Sucht einen String in einer Liste ohne Gross-/Kleinschreibung.
    private static int IndexOfIgnoreCase(List<string> list, string value)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (string.Equals(list[i], value, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    // Zeigt und bearbeitet die Portal-Einstellungen.
    private void RunSettingsMenu()
    {
        bool inMenu = true;
        while (inMenu)
        {
            Console.Clear();
            Console.WriteLine("=== Einstellungen ===");
            Console.WriteLine("1) Menue-Toene: " + (soundOn ? "An" : "Aus"));
            Console.WriteLine("2) Akzentfarbe: " + accentColor);
            Console.WriteLine("3) Zurueck");

            int choice = ReadInt("Auswahl: ", 1, 3);
            if (choice == 1)
            {
                soundOn = !soundOn;
                Console.WriteLine("Menue-Toene jetzt: " + (soundOn ? "An" : "Aus"));
                WaitForEnter();
            }
            else if (choice == 2)
            {
                RunAccentColorPicker();
            }
            else
            {
                inMenu = false;
            }
        }
    }

    // Erlaubt die Auswahl einer Menue-Akzentfarbe.
    private void RunAccentColorPicker()
    {
        Console.Clear();
        Console.WriteLine("=== Konsolenfarbwaehler ===");

        ConsoleColor[] options = new ConsoleColor[]
        {
            ConsoleColor.Cyan,
            ConsoleColor.Green,
            ConsoleColor.Magenta,
            ConsoleColor.Yellow,
            ConsoleColor.Blue,
            ConsoleColor.White
        };

        for (int i = 0; i < options.Length; i++)
        {
            Console.ForegroundColor = options[i];
            Console.WriteLine((i + 1) + ") " + options[i]);
            Console.ResetColor();
        }

        int choice = ReadInt("Auswahl: ", 1, options.Length);
        accentColor = options[choice - 1];
        Console.WriteLine("Neue Akzentfarbe: " + accentColor);
        WaitForEnter();
    }

    // Wartet, bis der Nutzer Enter drueckt.
    private static void WaitForEnter()
    {
        Console.WriteLine();
        Console.Write("Weiter mit Enter...");

        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                break;
            }
        }
    }

    // Spielt einen kurzen Navigations-Ton im Menue.
    private void PlayNavigationSound()
    {
        if (!soundOn)
        {
            return;
        }

        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            Console.Beep(1300, 40);
        }
        catch
        {
            // Falls Beep nicht klappt, ignorieren wir es ohne Absturz.
        }
    }

    // Laedt alle gespeicherten Portal-Daten aus einer JSON-Datei.
    private void LoadSettings()
    {
        if (!File.Exists(settingsPath))
        {
            return;
        }

        try
        {
            string json = File.ReadAllText(settingsPath);
            CasualPortalState? state = JsonSerializer.Deserialize<CasualPortalState>(json);
            if (state == null)
            {
                return;
            }

            soundOn = state.IsSoundEnabled;
            accentColor = state.AccentColor;

            notes.Clear();
            if (state.Notes != null)
            {
                for (int i = 0; i < state.Notes.Count; i++)
                {
                    notes.Add(state.Notes[i]);
                }
            }

            todos.Clear();
            if (state.Todos != null)
            {
                for (int i = 0; i < state.Todos.Count; i++)
                {
                    todos.Add(state.Todos[i]);
                }
            }

            expenses.Clear();
            if (state.Expenses != null)
            {
                for (int i = 0; i < state.Expenses.Count; i++)
                {
                    expenses.Add(state.Expenses[i]);
                }
            }
        }
        catch
        {
            // Defekte Settings ignorieren, damit die App trotzdem startet.
        }
    }

    // Speichert alle Portal-Daten in die JSON-Datei.
    private void SaveSettings()
    {
        try
        {
            CasualPortalState state = new CasualPortalState();
            state.IsSoundEnabled = soundOn;
            state.AccentColor = accentColor;
            state.Notes = notes;
            state.Todos = todos;
            state.Expenses = expenses;

            JsonSerializerOptions opts = new JsonSerializerOptions();
            opts.WriteIndented = true;
            string json = JsonSerializer.Serialize(state, opts);
            File.WriteAllText(settingsPath, json);
        }
        catch
        {
            // Beim Speichern soll die App nicht abstuerzen.
        }
    }

    // Liest eine ganze Zahl im erlaubten Bereich ein.
    private static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = (Console.ReadLine() ?? string.Empty).Trim();

            int value;
            bool ok = int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
            if (ok && value >= min && value <= max)
            {
                return value;
            }

            Console.WriteLine("Bitte eine ganze Zahl zwischen " + min + " und " + max + " eingeben.");
        }
    }

    // Liest eine Kommazahl ein und akzeptiert Punkt oder Komma.
    private static double ReadDouble(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = (Console.ReadLine() ?? string.Empty).Trim().Replace(',', '.');

            double value;
            bool ok = double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
            if (ok)
            {
                return value;
            }

            Console.WriteLine("Bitte eine gueltige Zahl eingeben.");
        }
    }

    // Liest ein Datum im Format YYYY-MM-DD ein.
    private static DateTime ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = (Console.ReadLine() ?? string.Empty).Trim();

            DateTime date;
            bool ok = DateTime.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
            if (ok)
            {
                return date;
            }

            Console.WriteLine("Format ungueltig. Beispiel: 2026-04-09");
        }
    }

    // Speichert alle Werte, die wir als JSON sichern wollen.
    private sealed class CasualPortalState
    {
        public bool IsSoundEnabled { get; set; } = true;
        public ConsoleColor AccentColor { get; set; } = ConsoleColor.Cyan;
        public List<string>? Notes { get; set; }
        public List<TodoItem>? Todos { get; set; }
        public List<ExpenseEntry>? Expenses { get; set; }
    }

    private enum TodoPriority
    {
        Low,
        Medium,
        High
    }

    private sealed class TodoItem
    {
        public string Text { get; set; } = string.Empty;
        public bool Done { get; set; }
        public TodoPriority Priority { get; set; } = TodoPriority.Medium;
    }

    private sealed class ExpenseEntry
    {
        public string Category { get; set; } = "Allgemein";
        public double Amount { get; set; }
    }

    private sealed class QuizQuestion
    {
        public QuizQuestion(string question, string[] answers, int correctIndex)
        {
            Question = question;
            Answers = answers;
            CorrectIndex = correctIndex;
        }

        public string Question { get; private set; }
        public string[] Answers { get; private set; }
        public int CorrectIndex { get; private set; }
    }
}
