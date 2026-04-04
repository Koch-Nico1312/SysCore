using System.Text;

namespace AdminApp;

/// <summary>Admin-Bereich nach erfolgreichem Login: Menüs und alle Admin-Programme.</summary>
public sealed partial class AdminPortal
{
    private nint _consoleInputHandle;

    /// <summary>Startet die Admin-Oberfläche (visuell oder vereinfacht bei Umleitung).</summary>
    public void Run()
    {
        if (Console.IsOutputRedirected)
            FuehreAdminNurTextAus();
        else
            FuehreAdminVisuellAus();
    }

    private void FuehreAdminVisuellAus()
    {
        StelleKonsolenfensterEin();
        Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        _consoleInputHandle = HoleStandardEingabeHandle();
        AdminMenueLeistenzaehlerInitialisieren();
        bool weiter = true;
        while (weiter)
        {
            int haupt = ZeigeHauptmenueUndWaehle();
            if (haupt == 1)
            {
                weiter = false;
                continue;
            }

            if (haupt != 0)
                continue;

            bool zurueckProgramme = false;
            while (!zurueckProgramme)
            {
                int prog = ZeigeProgrammListeUndWaehle();
                if (prog < 0 || prog >= AdminProgrammEintraege.Length)
                {
                    zurueckProgramme = true;
                    continue;
                }

                StarteProgrammNachIndex(prog);
                WarteAufWeiter();
            }
        }

        AdminMenueLeistenzaehlerFreigeben();
    }

    private void FuehreAdminNurTextAus()
    {
        bool weiter = true;
        while (weiter)
        {
            Console.Out.WriteLine("=== Admin ===");
            Console.Out.WriteLine("1) Programme starten");
            Console.Out.WriteLine("2) Beenden");
            Console.Out.Write("Wahl: ");
            string? w = Console.ReadLine();
            string wahl = w?.Trim() ?? "";
            if (wahl == "2")
            {
                weiter = false;
                continue;
            }

            if (wahl != "1")
                continue;

            bool zurueck = false;
            while (!zurueck)
            {
                TextMenueProgrammeAnzeigen();
                Console.Out.Write("Nummer (0 = Zurück): ");
                string? p = Console.ReadLine();
                string num = p?.Trim() ?? "";
                if (num == "0")
                {
                    zurueck = true;
                    continue;
                }

                if (!int.TryParse(num, out int idx) || idx < 1 || idx > AdminProgrammEintraege.Length)
                    continue;

                StarteProgrammNachIndex(idx - 1);
                Console.Out.WriteLine("Enter für weiter…");
                Console.ReadLine();
            }
        }
    }

    private static void TextMenueProgrammeAnzeigen()
    {
        Console.Out.WriteLine("--- Programme ---");
        for (int i = 0; i < AdminProgrammEintraege.Length; i++)
            Console.Out.WriteLine($"{i + 1,2}) {AdminProgrammEintraege[i]}");
        Console.Out.WriteLine(" 0) Zurück");
    }

    private static void StelleKonsolenfensterEin()
    {
        Console.CursorVisible = false;
        Console.ResetColor();
        if (OperatingSystem.IsWindows())
            ConsoleInputWindows.EnableMouseAndWindowInput();
    }

    private static nint HoleStandardEingabeHandle()
    {
        if (!OperatingSystem.IsWindows())
            return nint.Zero;
        return ConsoleInputWindows.GetStdHandle(ConsoleInputWindows.StdInputHandle);
    }

    private void StarteProgrammNachIndex(int index)
    {
        if (Console.IsOutputRedirected)
        {
            switch (index)
            {
                case 0: TaskManagerAusfuehren(); break;
                case 1: NotizbuchAusfuehren(); break;
                case 2: PasswortManagerAusfuehren(); break;
                case 3: KalenderAusfuehren(); break;
                case 4: EinheitenrechnerAusfuehren(); break;
                case 5: TaschenrechnerMitVerlaufAusfuehren(); break;
                case 6: CaesarWerkzeugAusfuehren(); break;
                case 7: TextAnalyzerAusfuehren(); break;
                case 8: WaehrungsrechnerAusfuehren(); break;
                case 9: QrCodeGeneratorAusfuehren(); break;
                case 10: DatumrechnerAusfuehren(); break;
                case 11: SystemMonitorNachPlattformStarten(); break;
                case 12: AsciiArtGeneratorAusfuehren(); break;
                case 13: FarbpalettenGeneratorAusfuehren(); break;
                case 14: MusikPlayerAusfuehren(); break;
            }

            return;
        }

        Console.CursorVisible = true;
        Console.Clear();
        Console.ResetColor();
        switch (index)
        {
            case 0: TaskManagerAusfuehren(); break;
            case 1: NotizbuchAusfuehren(); break;
            case 2: PasswortManagerAusfuehren(); break;
            case 3: KalenderAusfuehren(); break;
            case 4: EinheitenrechnerAusfuehren(); break;
            case 5: TaschenrechnerMitVerlaufAusfuehren(); break;
            case 6: CaesarWerkzeugAusfuehren(); break;
            case 7: TextAnalyzerAusfuehren(); break;
            case 8: WaehrungsrechnerAusfuehren(); break;
            case 9: QrCodeGeneratorAusfuehren(); break;
            case 10: DatumrechnerAusfuehren(); break;
            case 11: SystemMonitorNachPlattformStarten(); break;
            case 12: AsciiArtGeneratorAusfuehren(); break;
            case 13: FarbpalettenGeneratorAusfuehren(); break;
            case 14: MusikPlayerAusfuehren(); break;
        }
    }

    private static void WarteAufWeiter()
    {
        if (Console.IsOutputRedirected)
            return;
        Console.Out.WriteLine();
        Console.Out.WriteLine("Beliebige Taste zum Fortfahren…");
        Console.ReadKey(intercept: true);
    }

    private static readonly string[] HauptmenuePunkte =
    [
        "Programme starten",
        "Beenden"
    ];

    private static readonly string[] AdminProgrammEintraege =
    [
        "Task Manager (Aufgaben + Deadlines)",
        "Notizbuch (speichern / suchen)",
        "Passwort Manager (verschlüsselt)",
        "Kalender (Monatsansicht)",
        "Einheitenrechner",
        "Taschenrechner mit Verlauf",
        "Caesar / Verschlüsselungs-Tool",
        "Text Analyzer",
        "Währungsrechner (API)",
        "QR-Code Generator (Text)",
        "Datumrechner",
        "System Monitor (CPU, RAM)",
        "ASCII Art Generator",
        "Farbpaletten Generator",
        "Musik Player (MP3)"
    ];
}
