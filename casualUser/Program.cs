using System.Text;

namespace CasualUserApp;

// Diese Klasse ist der einfache Bereich für normale Benutzer (kein Admin).
// Ziel: Klare Menüs, verständliche Abläufe, keine komplizierten Konstrukte.
public sealed class CasualPortal
{
    // Speichert, ob Menü-Töne aktiv sind.
    private bool _isSoundEnabled = true;

    // Feste Menüeinträge für den Casual-Bereich.
    private static readonly string[] MainMenuItems =
    [
        "Notiz schreiben",
        "Einheitenrechner",
        "Datumrechner (Tage zwischen 2 Daten)",
        "Einstellungen",
        "Beenden"
    ];

    // Einstiegspunkt für den Casual-Bereich.
    public void Run()
    {
        PrepareConsole();
        LoadSettings();
        RunMainMenu();
        SaveSettings();
    }

    // Setzt die Konsole auf einen sauberen Startzustand.
    private static void PrepareConsole()
    {
        if (Console.IsOutputRedirected)
            return;

        Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        Console.Clear();
        Console.ResetColor();
        Console.CursorVisible = true;
    }

    // Hauptmenü-Schleife: läuft, bis der Benutzer "Beenden" wählt.
    private void RunMainMenu()
    {
        // Bei Umleitung (z. B. Ausgabe in Datei) funktioniert kein interaktives Pfeil-Menü gut.
        // Deshalb verwenden wir dann eine einfache Text-Variante.
        if (Console.IsOutputRedirected)
        {
            RunMainMenuTextOnly();
            return;
        }

        bool shouldContinue = true;
        while (shouldContinue)
        {
            int selectedIndex = SelectMainMenuWithArrows();

            if (selectedIndex == 0)
                RunNoteWriter();
            else if (selectedIndex == 1)
                RunUnitConverter();
            else if (selectedIndex == 2)
                RunDateCalculator();
            else if (selectedIndex == 3)
                RunSettingsMenu();
            else if (selectedIndex == 4)
                shouldContinue = false;

            if (shouldContinue)
                WaitForEnter();
        }
    }

    // Fallback-Menü ohne Pfeiltasten (nur Text und Zahlen).
    private void RunMainMenuTextOnly()
    {
        bool shouldContinue = true;
        while (shouldContinue)
        {
            Console.WriteLine("=== SysCore Casual User ===");
            Console.WriteLine("1) Notiz schreiben");
            Console.WriteLine("2) Einheitenrechner");
            Console.WriteLine("3) Datumrechner (Tage zwischen 2 Daten)");
            Console.WriteLine("4) Einstellungen");
            Console.WriteLine("5) Beenden");
            Console.Write("Wahl: ");

            string? input = Console.ReadLine();
            string choice = input == null ? "" : input.Trim();

            if (choice == "1")
                RunNoteWriter();
            else if (choice == "2")
                RunUnitConverter();
            else if (choice == "3")
                RunDateCalculator();
            else if (choice == "4")
                RunSettingsMenuTextOnly();
            else if (choice == "5")
                shouldContinue = false;
        }
    }

    // Zeigt ein farbiges Menü an und liefert den gewählten Index per Enter zurück.
    private int SelectMainMenuWithArrows()
    {
        int selectedIndex = 0;
        while (true)
        {
            DrawMainMenu(selectedIndex);
            DrawBottomStatusLine();

            ConsoleKeyInfo key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.UpArrow)
            {
                selectedIndex = selectedIndex <= 0 ? MainMenuItems.Length - 1 : selectedIndex - 1;
                PlayNavigationFeedback();
            }
            else if (key.Key == ConsoleKey.DownArrow)
            {
                selectedIndex = selectedIndex >= MainMenuItems.Length - 1 ? 0 : selectedIndex + 1;
                PlayNavigationFeedback();
            }
            else if (key.Key == ConsoleKey.Escape)
            {
                // Escape soll sofort "Beenden" auslösen.
                PlayConfirmFeedback();
                return MainMenuItems.Length - 1;
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                PlayConfirmFeedback();
                return selectedIndex;
            }
            else if (key.KeyChar >= '1' && key.KeyChar <= '5')
            {
                PlayConfirmFeedback();
                return key.KeyChar - '1';
            }
            else
            {
                // Bei ungültigen Tasten geben wir ein kleines Warnsignal.
                PlayInvalidKeyFeedback();
            }
        }
    }

    // Zeichnet Titel + Menüeinträge; markierter Eintrag wird farblich hervorgehoben.
    private static void DrawMainMenu(int selectedIndex)
    {
        DrawHeader();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("Mit Pfeiltasten wählen, Enter bestätigen, ESC = Beenden.");
        Console.WriteLine();
        Console.ResetColor();

        for (int i = 0; i < MainMenuItems.Length; i++)
        {
            bool isActive = i == selectedIndex;
            if (isActive)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(" > " + MainMenuItems[i]);
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("   " + MainMenuItems[i]);
                Console.ResetColor();
            }
        }
    }

    // Einstellungen-Menü (visuell).
    private void RunSettingsMenu()
    {
        if (Console.IsOutputRedirected)
        {
            RunSettingsMenuTextOnly();
            return;
        }

        bool shouldContinue = true;
        while (shouldContinue)
        {
            DrawHeader();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("=== Einstellungen ===");
            Console.WriteLine();
            Console.ResetColor();
            Console.WriteLine("1) Ton umschalten (aktuell: " + (_isSoundEnabled ? "EIN" : "AUS") + ")");
            Console.WriteLine("2) Zurück");
            Console.WriteLine();
            Console.Write("Wahl: ");

            ConsoleKeyInfo key = Console.ReadKey(intercept: true);
            Console.WriteLine(key.KeyChar);

            if (key.KeyChar == '1')
                ToggleSoundMode();
            else if (key.KeyChar == '2' || key.Key == ConsoleKey.Escape)
                shouldContinue = false;
        }
    }

    // Einstellungen-Menü (Text-Fallback).
    private void RunSettingsMenuTextOnly()
    {
        bool shouldContinue = true;
        while (shouldContinue)
        {
            Console.WriteLine("=== Einstellungen ===");
            Console.WriteLine("1) Ton umschalten (aktuell: " + (_isSoundEnabled ? "EIN" : "AUS") + ")");
            Console.WriteLine("2) Zurück");
            Console.Write("Wahl: ");

            string? input = Console.ReadLine();
            string choice = input == null ? "" : input.Trim();

            if (choice == "1")
                ToggleSoundMode();
            else if (choice == "2")
                shouldContinue = false;
        }
    }

    // Schaltet den Tonmodus um und zeigt den neuen Zustand an.
    private void ToggleSoundMode()
    {
        _isSoundEnabled = !_isSoundEnabled;
        SaveSettings();
        Console.WriteLine();
        Console.WriteLine(_isSoundEnabled ? "Ton ist jetzt: EIN" : "Ton ist jetzt: AUS");
    }

    // Zeichnet eine kleine Statuszeile am unteren Fensterrand.
    private void DrawBottomStatusLine()
    {
        if (Console.IsOutputRedirected)
            return;

        int width = Console.WindowWidth;
        int height = Console.WindowHeight;
        if (width <= 0 || height <= 0)
            return;

        string sound = _isSoundEnabled ? "Ton: EIN" : "Ton: AUS";
        string time = DateTime.Now.ToString("HH:mm:ss");
        string text = "Nico SysCore Casual | " + sound + " | " + time;
        if (text.Length > width)
            text = text.Substring(0, width);

        Console.SetCursorPosition(0, height - 1);
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write(text.PadRight(width));
        Console.ResetColor();
    }

    // Kleines Feedback beim Wechseln im Menü.
    private void PlayNavigationFeedback()
    {
        PlayShortBeep(600, 35);
    }

    // Deutliches Feedback bei Enter/Bestätigung.
    private void PlayConfirmFeedback()
    {
        PlayShortBeep(880, 70);
    }

    // Warnsignal bei ungültiger Eingabe.
    private void PlayInvalidKeyFeedback()
    {
        PlayShortBeep(250, 90);
    }

    // Spielt einen kurzen Ton ab. Auf manchen Systemen geht Console.Beep nicht:
    // dann fangen wir den Fehler einfach ab, damit das Programm trotzdem sauber läuft.
    private void PlayShortBeep(int frequency, int durationMs)
    {
        if (Console.IsOutputRedirected)
            return;
        if (!OperatingSystem.IsWindows())
            return;
        if (!_isSoundEnabled)
            return;

        try
        {
            Console.Beep(frequency, durationMs);
        }
        catch
        {
            // Absichtlich leer: Wenn Beep nicht unterstützt wird, machen wir nichts.
        }
    }

    // Zeichnet eine kleine Überschrift.
    private static void DrawHeader()
    {
        if (!Console.IsOutputRedirected)
            Console.Clear();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=== SysCore Casual User ===");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("  ____                 _ ");
        Console.WriteLine(" / ___| __ _ ___ _   _| |");
        Console.WriteLine("| |    / _` / __| | | | |");
        Console.WriteLine("| |___| (_| \\__ \\ |_| | |");
        Console.WriteLine(" \\____|\\__,_|___/\\__,_|_|");
        Console.ResetColor();
        Console.WriteLine();
    }

    // Einfache Notizfunktion: Text wird in AppData\SysCore\casual_notiz.txt gespeichert.
    private static void RunNoteWriter()
    {
        Console.WriteLine();
        Console.WriteLine("--- Notiz schreiben ---");
        Console.WriteLine("Text eingeben (leere Zeile beendet):");

        StringBuilder collector = new StringBuilder();
        while (true)
        {
            string? line = Console.ReadLine();
            if (line == null || line.Length == 0)
                break;

            collector.AppendLine(line);
        }

        string path = BuildNotePath();
        File.WriteAllText(path, collector.ToString(), Encoding.UTF8);
        Console.WriteLine("Notiz gespeichert: " + path);
    }

    // Kleiner Umrechner mit 4 typischen Fällen.
    private static void RunUnitConverter()
    {
        Console.WriteLine();
        Console.WriteLine("--- Einheitenrechner ---");
        Console.WriteLine("1) km -> m");
        Console.WriteLine("2) m -> km");
        Console.WriteLine("3) C -> F");
        Console.WriteLine("4) F -> C");
        Console.Write("Wahl: ");

        string? selectionInput = Console.ReadLine();
        string choice = selectionInput == null ? "" : selectionInput.Trim();

        Console.Write("Wert: ");
        string? valueText = Console.ReadLine();
        if (!double.TryParse(valueText, out double value))
        {
            Console.WriteLine("Ungültige Zahl.");
            return;
        }

        if (choice == "1")
            Console.WriteLine((value * 1000.0).ToString("0.###") + " m");
        else if (choice == "2")
            Console.WriteLine((value / 1000.0).ToString("0.###") + " km");
        else if (choice == "3")
            Console.WriteLine((value * 9.0 / 5.0 + 32.0).ToString("0.###") + " F");
        else if (choice == "4")
            Console.WriteLine(((value - 32.0) * 5.0 / 9.0).ToString("0.###") + " C");
        else
            Console.WriteLine("Ungültige Auswahl.");
    }

    // Berechnet den Abstand in Tagen zwischen zwei Datumswerten.
    private static void RunDateCalculator()
    {
        Console.WriteLine();
        Console.WriteLine("--- Datumrechner ---");
        Console.Write("Erstes Datum (dd.MM.yyyy): ");
        string? firstDateInput = Console.ReadLine();
        Console.Write("Zweites Datum (dd.MM.yyyy): ");
        string? secondDateInput = Console.ReadLine();

        if (!DateTime.TryParseExact(firstDateInput, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime firstDate))
        {
            Console.WriteLine("Erstes Datum ist ungültig.");
            return;
        }

        if (!DateTime.TryParseExact(secondDateInput, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime secondDate))
        {
            Console.WriteLine("Zweites Datum ist ungültig.");
            return;
        }

        int days = Math.Abs((secondDate.Date - firstDate.Date).Days);
        Console.WriteLine("Tage zwischen den Daten: " + days);
    }

    // Wartet, damit das Ergebnis vor dem nächsten Menü sichtbar bleibt.
    private static void WaitForEnter()
    {
        Console.WriteLine();
        Console.WriteLine("Enter für weiter ...");
        Console.ReadLine();
    }

    // Baut den Speicherpfad für die Casual-Notiz unter %AppData%\SysCore.
    private static string BuildNotePath()
    {
        string baseFolder = BuildDataFolderPath();
        return Path.Combine(baseFolder, "casual_notiz.txt");
    }

    // Baut den Pfad für die Konfigurationsdatei.
    private static string BuildSettingsPath()
    {
        string baseFolder = BuildDataFolderPath();
        return Path.Combine(baseFolder, "casual_config.txt");
    }

    // Gemeinsamer Datenordner für Casual-User-Dateien.
    private static string BuildDataFolderPath()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string folder = Path.Combine(appData, "SysCore");
        Directory.CreateDirectory(folder);
        return folder;
    }

    // Lädt Einstellungen aus der Datei.
    private void LoadSettings()
    {
        string path = BuildSettingsPath();
        if (!File.Exists(path))
            return;

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.StartsWith("sound_on=", StringComparison.OrdinalIgnoreCase))
            {
                string value = line.Substring("sound_on=".Length).Trim();
                _isSoundEnabled = value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    // Speichert Einstellungen in die Datei.
    private void SaveSettings()
    {
        string path = BuildSettingsPath();
        string value = _isSoundEnabled ? "1" : "0";
        string content = "sound_on=" + value + Environment.NewLine;
        File.WriteAllText(path, content, Encoding.UTF8);
    }
}

// Was macht diese Datei?
// - Stellt eine einfache Casual-User-Oberfläche bereit (kein Admin).
// - Enthält ein Menü mit Notiz, Einheitenrechner, Datumrechner und Einstellungen.
// - Speichert den Tonmodus dauerhaft in %AppData%\SysCore\casual_config.txt.
