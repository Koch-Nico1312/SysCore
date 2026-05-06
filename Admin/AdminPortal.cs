using System.Diagnostics;
using System.Text;

namespace AdminApp;

/// <summary>Admin-Bereich nach erfolgreichem Login: Menüs und alle Admin-Programme.</summary>
public sealed partial class AdminPortal
{
    // Hier ist einfach das Admin-Hauptding drin.
    // nint ist ein Zahlen-Typ für "native" Handles (betriebssystemnahe Werte).
    // Wir brauchen ihn wegen Windows-Konsolen-Eingabe über API-Aufrufe.
    // Das ist fortgeschritten und muss noch nicht komplett verstanden werden.
    private nint _consoleInputHandle;

    /// <summary>Startet Admin-Menue (normal oder Textmodus).</summary>
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
        bool run = true;
        while (run)
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
                ShowShortFeedback("Schule: Hier kannst du später deine Schul-Tools einbauen.");
                continue;
            }

            // 3 = Admin Tools -> verschachtelte Kategorien
            if (mainSelection == 3)
            {
                RunToolCategoryMenu();
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
                run = false;
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
            foreach (string bannerLine in AdminHauptmenüAsciiBanner)
                Console.Out.WriteLine(bannerLine);
            Console.Out.WriteLine("1) Admin Tools (Kategorien)");
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

            RunToolCategoryMenuTextOnly();
        }
    }

    private void RunToolCategoryMenuTextOnly()
    {
        bool backToCategories = false;
        while (!backToCategories)
        {
            Console.Out.WriteLine("--- Kategorien ---");
            for (int i = 0; i < CategoryNames.Length; i++)
                Console.Out.WriteLine($"{i + 1,2}) {CategoryNames[i]}");
            Console.Out.WriteLine(" 0) Zurück");
            Console.Out.Write("Wahl: ");
            string? c = Console.ReadLine();
            string cw = c?.Trim() ?? "";
            if (cw == "0") { backToCategories = true; continue; }
            if (!int.TryParse(cw, out int catIdx) || catIdx < 1 || catIdx > CategoryNames.Length)
                continue;
            catIdx--;

            int[] tools = CategoryToolIndices[catIdx];
            bool backToTools = false;
            while (!backToTools)
            {
                Console.Out.WriteLine($"--- {CategoryNames[catIdx]} ---");
                for (int i = 0; i < tools.Length; i++)
                    Console.Out.WriteLine($"{i + 1,2}) {AdminProgramEntries[tools[i]]}");
                Console.Out.WriteLine(" 0) Zurück");
                Console.Out.Write("Wahl: ");
                string? t = Console.ReadLine();
                string tw = t?.Trim() ?? "";
                if (tw == "0") { backToTools = true; continue; }
                if (!int.TryParse(tw, out int toolIdx) || toolIdx < 1 || toolIdx > tools.Length)
                    continue;
                StartProgramByIndex(tools[toolIdx - 1]);
                Console.Out.WriteLine("Enter für weiter…");
                Console.ReadLine();
            }
        }
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
                case 21: RunPomodoroTimer(); break;
                case 22: RunContactBook(); break;
                case 23: RunHashGenerator(); break;
                case 24: RunBase64Tool(); break;
                case 25: RunPasswordStrengthChecker(); break;
                case 26: RunProcessLister(); break;
                case 27: RunFileBrowser(); break;
                case 28: RunDuplicateFinder(); break;
                case 29: RunPortScanner(); break;
                case 30: RunHttpClientTool(); break;
                case 31: RunSnakeGame(); break;
                case 32: RunRockPaperScissors(); break;
                case 33: RunRandomGenerator(); break;
                case 34: RunBmiCalculator(); break;
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
            case 21: RunPomodoroTimer(); break;
            case 22: RunContactBook(); break;
            case 23: RunHashGenerator(); break;
            case 24: RunBase64Tool(); break;
            case 25: RunPasswordStrengthChecker(); break;
            case 26: RunProcessLister(); break;
            case 27: RunFileBrowser(); break;
            case 28: RunDuplicateFinder(); break;
            case 29: RunPortScanner(); break;
            case 30: RunHttpClientTool(); break;
            case 31: RunSnakeGame(); break;
            case 32: RunRockPaperScissors(); break;
            case 33: RunRandomGenerator(); break;
            case 34: RunBmiCalculator(); break;
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
        "Admin Tools",
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
        "🤖 Gemini AI Chat",
        "⏱️ Pomodoro-Timer",
        "📇 Kontaktbuch",
        "#️⃣ Hash-Generator",
        "🔤 Base64 En/Decoder",
        "🔒 Passwort-Stärke-Checker",
        "📈 Prozess-Lister",
        "📁 Datei-Browser",
        "🔍 Duplicate Finder",
        "🔌 Port-Scanner",
        "🌐 HTTP-Client",
        "🐍 Snake",
        "✊ Schere-Stein-Papier",
        "🎰 Zufallsgenerator",
        "⚖️ BMI / Fitness-Rechner"
    ];

    private static readonly string[] CategoryNames =
    [
        "🗂️  Produktivität",
        "🛠️  Tools & Rechner",
        "💻 System & Netzwerk",
        "🎨 Kreativ & Spiele",
        "💰 Finanzen",
        "🌐 Web, Medien & AI"
    ];

    private static readonly int[][] CategoryToolIndices =
    [
        [0, 1, 2, 3, 21, 22],
        [4, 5, 6, 7, 8, 9, 10, 15, 23, 24, 25],
        [11, 26, 27, 28, 29, 30],
        [12, 13, 14, 16, 17, 31, 32, 33],
        [18],
        [19, 20]
    ];

    // Öffnet eine Webseite im Standardbrowser.
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
