using System.Diagnostics;
using System.Text;

namespace AdminApp;

/// <summary>Admin-Bereich nach erfolgreichem Login: Menüs und alle Admin-Programme.</summary>
// WICHTIG FÜR DICH:
// Diese Klasse ist "partial", weil ihr Code auf mehrere Dateien verteilt ist.
// Das ist nur eine Aufteilung für bessere Übersicht. Logisch ist es am Ende EINE Klasse.
// Du kannst das später lernen; für jetzt reicht: Alle "AdminPortal.*.cs" gehören zusammen.
//
// "sealed" bedeutet: Von dieser Klasse kann keine Unterklasse abgeleitet werden.
// Das ist hier nur eine Design-Entscheidung und ändert nichts am Grundablauf.
public sealed partial class AdminPortal
{
    // nint ist ein Zahlen-Typ für "native" Handles (betriebssystemnahe Werte).
    // Wir brauchen ihn wegen Windows-Konsolen-Eingabe über API-Aufrufe.
    // Das ist fortgeschritten und muss noch nicht komplett verstanden werden.
    private nint _consoleInputHandle;

    /// <summary>Startet die Admin-Oberfläche (visuell oder vereinfacht bei Umleitung).</summary>
    public void Run()
    {
        // Wenn Ausgabe umgeleitet ist (z. B. in eine Datei), nehmen wir ein einfaches Textmenü.
        if (Console.IsOutputRedirected)
            RunAdminTextOnlyMode();
        else
            // Sonst benutzen wir das visuelle Menü.
            RunAdminVisualMode();
    }

    // Hauptschleife für die visuelle Admin-Oberfläche.
    private void RunAdminVisualMode()
    {
        ConfigureConsoleWindow();
        Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        _consoleInputHandle = GetStandardInputHandle();
        InitializeAdminMenuPerformanceCounters();
        bool shouldContinue = true;
        while (shouldContinue)
        {
            int mainSelection = ShowMainMenuAndSelect();
            // 0 = Youtube schauen
            if (mainSelection == 0)
            {
                OpenUrlInBrowser("https://www.youtube.com");
                continue;
            }

            // 1 = Serie schauen
            if (mainSelection == 1)
            {
                OpenUrlInBrowser("https://www.netflix.com");
                continue;
            }

            // 2 = Schule (Platzhalter)
            if (mainSelection == 2)
            {
                ShowShortFeedback("Schule: Hier kannst du spaeter deine Schul-Tools einbauen.");
                continue;
            }

            // 3 = PC -> bestehende Programmliste
            if (mainSelection == 3)
            {
                bool backToPrograms = false;
                while (!backToPrograms)
                {
                    int selectedProgram = ShowProgramListAndSelect();
                    if (selectedProgram < 0 || selectedProgram >= AdminProgramEntries.Length)
                    {
                        backToPrograms = true;
                        continue;
                    }

                    StartProgramByIndex(selectedProgram);
                    WaitForContinue();
                }
                continue;
            }

            // 4 = Farbthema waehlen
            if (mainSelection == 4)
            {
                RunColorPaletteGenerator();
                continue;
            }

            // 5 = Musik
            if (mainSelection == 5)
            {
                StartProgramByIndex(14);
                WaitForContinue();
                continue;
            }

            // 6 = Beenden
            if (mainSelection == 6)
            {
                shouldContinue = false;
                continue;
            }
        }

        DisposeAdminMenuPerformanceCounters();
    }

    // Vereinfachte Variante ohne Maus/Live-Elemente.
    private void RunAdminTextOnlyMode()
    {
        bool shouldContinue = true;
        while (shouldContinue)
        {
            foreach (string bannerLine in AdminHauptmenueAsciiBanner)
                Console.Out.WriteLine(bannerLine);
            Console.Out.WriteLine("1) Programme starten");
            Console.Out.WriteLine("2) Beenden");
            Console.Out.Write("Wahl: ");
            string? w = Console.ReadLine();
            string wahl = w?.Trim() ?? "";
            if (wahl == "2")
            {
                shouldContinue = false;
                continue;
            }

            if (wahl != "1")
                continue;

            bool goBack = false;
            while (!goBack)
            {
                ShowProgramMenuTextOnly();
                Console.Out.Write("Nummer (0 = Zurück): ");
                string? p = Console.ReadLine();
                string num = p?.Trim() ?? "";
                if (num == "0")
                {
                    goBack = true;
                    continue;
                }

                if (!int.TryParse(num, out int idx) || idx < 1 || idx > AdminProgramEntries.Length)
                    continue;

                StartProgramByIndex(idx - 1);
                Console.Out.WriteLine("Enter für weiter…");
                Console.ReadLine();
            }
        }
    }

    // Zeigt die Programmliste in der Text-Variante.
    private static void ShowProgramMenuTextOnly()
    {
        Console.Out.WriteLine("--- Programme ---");
        for (int i = 0; i < AdminProgramEntries.Length; i++)
            Console.Out.WriteLine($"{i + 1,2}) {AdminProgramEntries[i]}");
        Console.Out.WriteLine(" 0) Zurück");
    }

    // Grundzustand der Konsole (Farben/Mausmodus) vorbereiten.
    private static void ConfigureConsoleWindow()
    {
        Console.CursorVisible = false;
        Console.ResetColor();
        if (OperatingSystem.IsWindows())
            ConsoleInputWindows.EnableMouseAndWindowInput();
    }

    // Holt ein Betriebssystem-Handle für Konsolen-Eingabe.
    // Fortgeschrittenes Thema (Windows-API), deshalb in eigene Methode ausgelagert.
    private static nint GetStandardInputHandle()
    {
        if (!OperatingSystem.IsWindows())
            return nint.Zero;
        return ConsoleInputWindows.GetStdHandle(ConsoleInputWindows.StdInputHandle);
    }

    // Startet das gewählte Unterprogramm.
    private void StartProgramByIndex(int index)
    {
        if (Console.IsOutputRedirected)
        {
            switch (index)
            {
                case 0: RunTaskManager(); break;
                case 1: RunNotebook(); break;
                case 2: RunPasswordManager(); break;
                case 3: RunCalendar(); break;
                case 4: RunUnitConverter(); break;
                case 5: RunCalculatorWithHistory(); break;
                case 6: RunCaesarTool(); break;
                case 7: RunTextAnalyzer(); break;
                case 8: RunCurrencyConverter(); break;
                case 9: RunQrCodeGenerator(); break;
                case 10: RunDateCalculator(); break;
                case 11: RunSystemMonitorByPlatform(); break;
                case 12: RunAsciiArtGenerator(); break;
                case 13: RunColorPaletteGenerator(); break;
                case 14: RunMusicPlayer(); break;
                case 15: RunWorldClockTool(); break;
                case 16: RunDiceSimulator(); break;
                case 17: RunQuizGame(); break;
                case 18: RunExpenseTracker(); break;
                case 19: RunExternalProgramLauncher(); break;
                case 20: RunGeminiChatModule(); break;
            }

            return;
        }

        Console.CursorVisible = true;
        Console.Clear();
        Console.ResetColor();
        switch (index)
        {
            case 0: RunTaskManager(); break;
            case 1: RunNotebook(); break;
            case 2: RunPasswordManager(); break;
            case 3: RunCalendar(); break;
            case 4: RunUnitConverter(); break;
            case 5: RunCalculatorWithHistory(); break;
            case 6: RunCaesarTool(); break;
            case 7: RunTextAnalyzer(); break;
            case 8: RunCurrencyConverter(); break;
            case 9: RunQrCodeGenerator(); break;
            case 10: RunDateCalculator(); break;
            case 11: RunSystemMonitorByPlatform(); break;
            case 12: RunAsciiArtGenerator(); break;
            case 13: RunColorPaletteGenerator(); break;
            case 14: RunMusicPlayer(); break;
            case 15: RunWorldClockTool(); break;
            case 16: RunDiceSimulator(); break;
            case 17: RunQuizGame(); break;
            case 18: RunExpenseTracker(); break;
            case 19: RunExternalProgramLauncher(); break;
            case 20: RunGeminiChatModule(); break;
        }
    }

    // Kleine Pause nach einem Programmstart (nur in der visuellen Variante).
    private static void WaitForContinue()
    {
        if (Console.IsOutputRedirected)
            return;
        Console.Out.WriteLine();
        Console.Out.WriteLine("Beliebige Taste zum Fortfahren…");
        Console.ReadKey(intercept: true);
    }

    private static readonly string[] MainMenuItems =
    [
        "Youtube schauen",
        "Serie schauen",
        "Schule",
        "PC",
        "Farbthema waehlen",
        "Musik",
        "Beenden"
    ];

    private static readonly string[] AdminProgramEntries =
    [
        "📋 Task Manager (Aufgaben + Deadlines)",
        "📓 Notizbuch (speichern / suchen)",
        "🔑 Passwort Manager (verschlüsselt)",
        "📅 Kalender (Monatsansicht)",
        "🧮 Einheitenrechner",
        "🖩 Taschenrechner mit Verlauf",
        "Caesar / Verschlüsselungs-Tool",
        "Text Analyzer",
        "💱 Währungsrechner (API)",
        "QR-Code Generator (Text)",
        "Datumrechner",
        "System Monitor (CPU, RAM)",
        "ASCII Art Generator",
        "Farbpaletten Generator",
        "Musik Player (MP3)",
        "🌍 Weltzeit",
        "🎲 Würfelsimulator",
        "❓ Quiz",
        "📊 Ausgaben-Tracker",
        "🚀 Programmstarter",
        "🤖 Gemini AI Chat"
    ];

    // Oeffnet eine Webseite im Standardbrowser.
    private static void OpenUrlInBrowser(string url)
    {
        try
        {
            ProcessStartInfo info = new()
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(info);
        }
        catch
        {
            // Wenn Browserstart fehlschlaegt, zeigen wir eine kurze Meldung.
            ShowShortFeedback("Browser konnte nicht gestartet werden.");
        }
    }

    // Kurzer Hinweistext im normalen Konsolenmodus.
    private static void ShowShortFeedback(string text)
    {
        Console.Clear();
        Console.ResetColor();
        Console.WriteLine(text);
        Console.WriteLine();
        Console.WriteLine("Beliebige Taste zum Fortfahren...");
        Console.ReadKey(intercept: true);
    }
}

// Was macht diese Datei?
// - Startet den Admin-Bereich.
// - Schaltet zwischen visueller und einfacher Text-Oberfläche um.
// - Startet die einzelnen Admin-Tools über ihren Menü-Index.
