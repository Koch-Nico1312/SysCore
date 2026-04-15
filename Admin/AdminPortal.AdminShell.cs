using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace AdminApp;

public sealed partial class AdminPortal
{
    // WICHTIG:
    // PerformanceCounter ist eine Windows-spezifische API für Systemwerte (CPU/RAM).
    // Das ist fortgeschritten, wir lassen es für gleiche Funktionalität im Projekt drin.
    private PerformanceCounter? _adminMenuCpu;
    private PerformanceCounter? _adminMenuRam;
    private long _adminMenuLastSystemBarMs;
    private int _adminRetroWertZeitX;
    private int _adminRetroWertZeitY;
    private int _adminRetroWertDatumX;
    private int _adminRetroWertDatumY;
    private int _adminRetroVolumeX;
    private int _adminRetroVolumeY;
    private int _adminRetroSongX;
    private int _adminRetroSongY;
    private int _adminRetroRechtsStartX;
    private int _adminRetroRechtsBreite;
    private int _adminRetroLautstaerke = 50;
    private string _adminRetroSongText = "♪ Kein Song";
    private ConsoleColor _themePrimary = ConsoleColor.Green;
    private ConsoleColor _themeAccent = ConsoleColor.DarkGreen;
    private ConsoleColor _themeBackground = ConsoleColor.Black;
    private ConsoleColor _themeHighlightForeground = ConsoleColor.White;
    private ConsoleColor _themeHighlightBackground = ConsoleColor.DarkBlue;

    private static readonly string[] AdminHauptmenueAsciiBanner =
    [
        @"  /$$$$$$        /$$               /$$          ",
        @" /$$__  $$      | $$              |__/          ",
        @"| $$  \ $$  /$$$$$$$ /$$$$$$/$$$$  /$$ /$$$$$$$ ",
        @"| $$$$$$$$ /$$__  $$| $$_  $$_  $$| $$| $$__  $$",
        @"| $$__  $$| $$  | $$| $$ \ $$ \ $$| $$| $$  \ $$",
        @"| $$  | $$| $$  | $$| $$ | $$ | $$| $$| $$  | $$",
        @"| $$  | $$|  $$$$$$$| $$ | $$ | $$| $$| $$  | $$",
        @"|__/  |__/ \_______/|__/ |__/ |__/|__/|__/  |__/",
    ];

    // Berechnet die obere Zeile der Systemleiste (unteres Viertel der Konsole).
    private static int CalculateAdminSystemBarFirstLine(int fensterHeight)
    {
        if (fensterHeight <= 0)
            return 0;
        // Die Systemleiste nutzt immer die letzten 2 Zeilen,
        // damit im Menuebereich maximal viel Platz bleibt.
        return Math.Max(0, fensterHeight - 2);
    }

    // Startspalte für Quadrant 4: rechte Hälfte der Konsole.
    private static int CalculateAdminSystemBarStartColumn(int windowWidth)
    {
        if (windowWidth <= 0)
            return 0;
        int s = windowWidth / 2;
        if (s >= windowWidth)
            s = windowWidth - 1;
        return Math.Max(0, s);
    }

    // Breite vom rechten Bereich (ab Startspalte bis Fensterende).
    private static int CalculateAdminSystemBarWidth(int windowWidth, int startColumn)
    {
        if (windowWidth <= 0)
            return 1;
        int breite = windowWidth - startColumn;
        return Math.Max(1, breite);
    }

    // Zeichnet das ASCII-Banner im Hauptmenü.
    private void DrawAdminMainMenuAsciiBanner(int startZeile, int fensterBreite)
    {
        ConsoleColor bannerAkzent = _themeAccent == _themeBackground ? _themePrimary : _themeAccent;
        int h = Console.WindowHeight;
        if (h <= 0) h = 25;
        for (int i = 0; i < AdminHauptmenueAsciiBanner.Length; i++)
        {
            string zeile = AdminHauptmenueAsciiBanner[i];
            int row = startZeile + i;
            if (row < 0 || row >= h)
                continue;
            int dw = EstimateDisplayWidth(zeile);
            int pad = Math.Max(0, (fensterBreite - dw) / 2);
            Console.SetCursorPosition(0, row);
            Console.Write(new string(' ', fensterBreite));
            Console.SetCursorPosition(pad, row);
            Console.ForegroundColor = i % 2 == 0 ? bannerAkzent : _themePrimary;
            Console.BackgroundColor = _themeBackground;
            Console.Write(zeile);
        }

        Console.ForegroundColor = _themePrimary;
        Console.BackgroundColor = _themeBackground;
    }

    // Zeichnet die zwei Zeilen mit Systeminfos.
    private void DrawAdminMenuSystemBar(int windowWidth, int windowHeight)
    {
        int r0 = CalculateAdminSystemBarFirstLine(windowHeight);
        int r1 = r0 + 1;
        int c0 = CalculateAdminSystemBarStartColumn(windowWidth);
        int teilBreite = CalculateAdminSystemBarWidth(windowWidth, c0);

        ClearAdminMenuSystemLine(c0, teilBreite, r0);
        if (r1 < windowHeight)
            ClearAdminMenuSystemLine(c0, teilBreite, r1);

        string zeile1 = BuildAdminMenuSystemTextLine1();
        string zeile2 = BuildAdminMenuSystemTextLine2();
        WriteAdminMenuSystemLine(c0, teilBreite, r0, zeile1);
        if (r1 < windowHeight)
            WriteAdminMenuSystemLine(c0, teilBreite, r1, zeile2);
    }

    // Aktualisiert die Systeminfos in zeitlichem Abstand.
    private void UpdateAdminMenuSystemBar(int windowWidth, int windowHeight)
    {
        long jetzt = Environment.TickCount64;
        if (jetzt - _adminMenuLastSystemBarMs < 550)
            return;
        _adminMenuLastSystemBarMs = jetzt;

        int r0 = CalculateAdminSystemBarFirstLine(windowHeight);
        int r1 = r0 + 1;
        int c0 = CalculateAdminSystemBarStartColumn(windowWidth);
        int teilBreite = CalculateAdminSystemBarWidth(windowWidth, c0);
        string zeile1 = BuildAdminMenuSystemTextLine1();
        string zeile2 = BuildAdminMenuSystemTextLine2();
        WriteAdminMenuSystemLine(c0, teilBreite, r0, zeile1);
        if (r1 < windowHeight)
            WriteAdminMenuSystemLine(c0, teilBreite, r1, zeile2);
    }

    // Löscht einen Abschnitt in einer Zeile.
    private static void ClearAdminMenuSystemLine(int startColumn, int fieldWidth, int row)
    {
        if (row < 0 || row >= Console.WindowHeight)
            return;
        if (startColumn < 0 || startColumn >= Console.WindowWidth)
            return;
        Console.SetCursorPosition(startColumn, row);
        Console.Write(new string(' ', fieldWidth));
    }

    // Schreibt einen Text in einen Abschnitt einer Zeile.
    private static void WriteAdminMenuSystemLine(int startColumn, int fieldWidth, int row, string text)
    {
        if (row < 0 || row >= Console.WindowHeight)
            return;
        if (startColumn < 0 || startColumn >= Console.WindowWidth)
            return;
        if (text.Length > fieldWidth)
            text = text[..fieldWidth];
        Console.SetCursorPosition(startColumn, row);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(text.PadRight(fieldWidth));
        Console.ResetColor();
    }

    // Erzeugt die erste Infozeile (CPU/RAM oder Fallback).
    private string BuildAdminMenuSystemTextLine1()
    {
        if (OperatingSystem.IsWindows() && _adminMenuCpu != null && _adminMenuRam != null)
        {
            float cpu = _adminMenuCpu.NextValue();
            float ramMb = _adminMenuRam.NextValue();
            float ramGb = ramMb / 1024f;
            return string.Format(CultureInfo.InvariantCulture,
                "⚡ CPU: {0:0.#}%   💾 RAM: {1:0.0} GB frei   🕒 {2}",
                cpu, ramGb, DateTime.Now.ToString("HH:mm:ss", CultureInfo.CurrentCulture));
        }

        return BuildAdminMenuSystemFallbackLine1();
    }

    // Fallback, falls keine Windows-PerformanceCounter verfügbar sind.
    private static string BuildAdminMenuSystemFallbackLine1()
    {
        Process p = Process.GetCurrentProcess();
        long mb = p.WorkingSet64 / (1024 * 1024);
        double gb = mb / 1024.0;
        return string.Format(CultureInfo.CurrentCulture,
            "⚡ CPU: ?   💾 RAM: {0:0.0} GB frei   🕒 {1}",
            gb, DateTime.Now.ToString("HH:mm:ss", CultureInfo.CurrentCulture));
    }

    // Erzeugt die zweite Infozeile mit Rechner/OS/Bitness.
    private static string BuildAdminMenuSystemTextLine2()
    {
        string name = Environment.MachineName;
        string os = Environment.OSVersion.Platform.ToString();
        string bit = Environment.Is64BitOperatingSystem ? "64-Bit" : "32-Bit";
        return string.Format(CultureInfo.CurrentCulture, "╔══ {0} ══╗   ║ {1} ║   ╚══ {2} ══╝", name, os, bit);
    }

    // Initialisiert Counter für CPU/RAM.
    private void InitializeAdminMenuPerformanceCounters()
    {
        DisposeAdminMenuPerformanceCounters();
        if (!OperatingSystem.IsWindows())
            return;

        _adminMenuCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _adminMenuRam = new PerformanceCounter("Memory", "Available MBytes");
        _ = _adminMenuCpu.NextValue();
        Thread.Sleep(250);
    }

    // Gibt Counter wieder frei.
    private void DisposeAdminMenuPerformanceCounters()
    {
        _adminMenuCpu?.Dispose();
        _adminMenuRam?.Dispose();
        _adminMenuCpu = null;
        _adminMenuRam = null;
    }

    // Prüft, ob das ASCII-Banner in die aktuelle Breite passt.
    private static bool AdminAsciiBannerPasstInBreite(int fensterBreite)
    {
        int max = 0;
        foreach (string z in AdminHauptmenueAsciiBanner)
        {
            int w = EstimateDisplayWidth(z);
            if (w > max) max = w;
        }

        return max <= fensterBreite - 2;
    }

    // Zeichnet das neue 60/40-Retro-Layout fuer das Hauptmenue.
    private void DrawAdminRetroLayout(IReadOnlyList<string> zeilen, int markiert)
    {
        int breite = Console.WindowWidth;
        int hoehe = Console.WindowHeight;
        if (breite <= 0) breite = 80;
        if (hoehe <= 0) hoehe = 25;

        int linkeBreite = (breite * 60) / 100;
        int rechteStart = linkeBreite;
        int rechteBreite = Math.Max(1, breite - rechteStart);
        _adminRetroRechtsStartX = rechteStart;
        _adminRetroRechtsBreite = rechteBreite;

        Console.Clear();
        Console.ResetColor();
        Console.ForegroundColor = _themePrimary;
        Console.BackgroundColor = _themeBackground;

        for (int y = 0; y < hoehe - 1; y++)
        {
            if (linkeBreite - 1 >= 0 && linkeBreite - 1 < breite)
            {
                Console.SetCursorPosition(linkeBreite - 1, y);
                Console.Write("│");
            }
        }

        // Zeichnet das Admin-ASCII-Banner im linken Bereich.
        const int bannerStartZeile = 1;
        const int bannerAbstandZumMenue = 2;
        if (hoehe > bannerStartZeile + 1)
        {
            DrawAdminMainMenuAsciiBanner(startZeile: bannerStartZeile, fensterBreite: Math.Max(1, linkeBreite - 1));
        }

        // Menue startet standardmaessig unter dem Banner.
        // Bei kleinen Fenstern wird es so weit wie noetig nach oben geschoben,
        // bleibt aber oberhalb der Hilfezeile.
        int bevorzugterMenueStart = bannerStartZeile + AdminHauptmenueAsciiBanner.Length + bannerAbstandZumMenue;
        int spaetesterMenueStart = Math.Max(1, hoehe - 2 - zeilen.Count);
        _menuStartRow = Math.Min(bevorzugterMenueStart, spaetesterMenueStart);
        for (int i = 0; i < zeilen.Count; i++)
        {
            int y = _menuStartRow + i;
            if (y < 0 || y >= hoehe - 2)
                continue;

            Console.SetCursorPosition(1, y);
            Console.Write(new string(' ', Math.Max(1, linkeBreite - 2)));
            Console.SetCursorPosition(3, y);
            if (i == markiert)
            {
                Console.ForegroundColor = _themeHighlightForeground;
                Console.BackgroundColor = _themeHighlightBackground;
                Console.Write("» " + zeilen[i]);
                Console.ResetColor();
                Console.ForegroundColor = _themePrimary;
                Console.BackgroundColor = _themeBackground;
            }
            else
            {
                Console.Write("  " + zeilen[i]);
            }
        }

        string hilfe = "+/- = Lautstaerke   N = naechster Song   P = vorheriger Song   M = Musik-Ordner waehlen   Klick = Enter";
        if (hilfe.Length > breite - 2)
            hilfe = hilfe[..(breite - 2)];
        Console.SetCursorPosition(1, hoehe - 2);
        Console.ForegroundColor = _themeAccent;
        Console.Write(hilfe.PadRight(breite - 2));
        Console.ForegroundColor = _themePrimary;

        int infoY = 2;
        WriteAdminRetroRightAligned(infoY + 0, BuildAdminRetroStorageText());
        WriteAdminRetroRightAligned(infoY + 1, "Laufwerk: C:\\");
        WriteAdminRetroRightAligned(infoY + 2, "Benutzer: " + Environment.UserName);
        WriteAdminRetroRightAligned(infoY + 3, BuildAdminRetroRamText());

        string zeitLabel = "Uhrzeit: ";
        string datumLabel = "Datum: ";
        string zeitPlatzhalter = zeitLabel + "00:00:00";
        string datumPlatzhalter = datumLabel + "00.00.0000";
        int zeitZeile = infoY + 4;
        int datumZeile = infoY + 5;
        WriteAdminRetroRightAligned(zeitZeile, zeitPlatzhalter);
        WriteAdminRetroRightAligned(datumZeile, datumPlatzhalter);

        _adminRetroWertZeitY = zeitZeile;
        _adminRetroWertDatumY = datumZeile;
        _adminRetroWertZeitX = _adminRetroRechtsStartX + _adminRetroRechtsBreite - zeitPlatzhalter.Length + zeitLabel.Length;
        _adminRetroWertDatumX = _adminRetroRechtsStartX + _adminRetroRechtsBreite - datumPlatzhalter.Length + datumLabel.Length;

        _adminRetroSongY = infoY + 8;
        _adminRetroVolumeY = _adminRetroSongY + 1;
        WriteAdminRetroSongLine();
        WriteAdminRetroVolumeLine();
        UpdateAdminRetroTimeAndDate(immediate: true);
    }

    // Schreibt Text rechtsbuendig innerhalb der rechten Spalte.
    private void WriteAdminRetroRightAligned(int row, string text)
    {
        if (row < 0 || row >= Console.WindowHeight)
            return;
        if (_adminRetroRechtsBreite <= 0)
            return;
        if (text.Length > _adminRetroRechtsBreite)
            text = text[.._adminRetroRechtsBreite];
        int x = _adminRetroRechtsStartX + _adminRetroRechtsBreite - text.Length;
        if (x < _adminRetroRechtsStartX)
            x = _adminRetroRechtsStartX;
        Console.SetCursorPosition(x, row);
        Console.ForegroundColor = _themePrimary;
        Console.Write(text.PadRight(_adminRetroRechtsStartX + _adminRetroRechtsBreite - x));
    }

    // Aktualisiert nur Uhrzeit und Datum, ohne kompletten Bildschirm neu zu zeichnen.
    private void UpdateAdminRetroTimeAndDate(bool immediate)
    {
        long jetzt = Environment.TickCount64;
        if (!immediate && jetzt - _adminMenuLastSystemBarMs < 500)
            return;
        _adminMenuLastSystemBarMs = jetzt;

        string zeit = DateTime.Now.ToString("HH:mm:ss", CultureInfo.CurrentCulture);
        string datum = DateTime.Now.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture);
        WriteAdminRetroValue(_adminRetroWertZeitX, _adminRetroWertZeitY, 8, zeit);
        WriteAdminRetroValue(_adminRetroWertDatumX, _adminRetroWertDatumY, 10, datum);
    }

    // Schreibt ein Wertfeld mit fixer Breite.
    private void WriteAdminRetroValue(int x, int y, int fieldWidth, string value)
    {
        if (y < 0 || y >= Console.WindowHeight)
            return;
        if (x < 0 || x >= Console.WindowWidth)
            return;
        if (value.Length > fieldWidth)
            value = value[..fieldWidth];
        Console.SetCursorPosition(x, y);
        Console.ForegroundColor = _themePrimary;
        Console.Write(value.PadRight(fieldWidth));
    }

    // Schreibt die Song-Zeile im rechten Bereich.
    private void WriteAdminRetroSongLine()
    {
        WriteAdminRetroRightAligned(_adminRetroSongY, _adminRetroSongText);
        string voll = _adminRetroSongText;
        if (voll.Length > _adminRetroRechtsBreite)
            voll = voll[.._adminRetroRechtsBreite];
        _adminRetroSongX = _adminRetroRechtsStartX + _adminRetroRechtsBreite - voll.Length;
    }

    // Schreibt die Lautstaerke-Zeile im rechten Bereich.
    private void WriteAdminRetroVolumeLine()
    {
        string zeile = BuildAdminRetroVolumeText();
        WriteAdminRetroRightAligned(_adminRetroVolumeY, zeile);
        string voll = zeile;
        if (voll.Length > _adminRetroRechtsBreite)
            voll = voll[.._adminRetroRechtsBreite];
        _adminRetroVolumeX = _adminRetroRechtsStartX + _adminRetroRechtsBreite - voll.Length;
    }

    // Baut den ASCII-Balken fuer Lautstaerke.
    private string BuildAdminRetroVolumeText()
    {
        const int gesamt = 10;
        int voll = (_adminRetroLautstaerke * gesamt) / 100;
        if (voll < 0) voll = 0;
        if (voll > gesamt) voll = gesamt;
        string balken = "";
        for (int i = 0; i < gesamt; i++)
        {
            balken += i < voll ? "█" : "░";
        }

        return "Volume: [" + balken + "] " + _adminRetroLautstaerke + "%";
    }

    // Erhoeht oder verringert die Lautstaerke in 5er-Schritten.
    private void ChangeAdminRetroVolume(int delta)
    {
        _adminRetroLautstaerke += delta;
        if (_adminRetroLautstaerke < 0) _adminRetroLautstaerke = 0;
        if (_adminRetroLautstaerke > 100) _adminRetroLautstaerke = 100;
        WriteAdminRetroVolumeLine();
    }

    // Setzt einen einfachen Song-Text und zeichnet ihn neu.
    private void SetAdminRetroSongText(string text)
    {
        _adminRetroSongText = text;
        WriteAdminRetroSongLine();
    }

    // Liefert Speicher-Info fuer C:\ im gewuenschten Format.
    private static string BuildAdminRetroStorageText()
    {
        try
        {
            DriveInfo c = new("C");
            if (!c.IsReady)
                return "Speicher: Laufwerk nicht bereit";
            long freiGb = c.AvailableFreeSpace / (1024L * 1024L * 1024L);
            long gesamtGb = c.TotalSize / (1024L * 1024L * 1024L);
            return string.Format(CultureInfo.CurrentCulture, "Speicher: {0} GB frei / {1} GB", freiGb, gesamtGb);
        }
        catch
        {
            return "Speicher: nicht verfuegbar";
        }
    }

    // Liefert freien RAM in MB.
    private string BuildAdminRetroRamText()
    {
        if (OperatingSystem.IsWindows() && _adminMenuRam != null)
        {
            float ramMb = _adminMenuRam.NextValue();
            return string.Format(CultureInfo.CurrentCulture, "RAM frei: {0:0} MB", ramMb);
        }

        Process p = Process.GetCurrentProcess();
        long mb = p.WorkingSet64 / (1024 * 1024);
        return string.Format(CultureInfo.CurrentCulture, "RAM frei: ca. {0} MB", mb);
    }

    // Aktualisiert das Farbschema des Admin-Retro-Layouts.
    private void SetAdminTheme(
        ConsoleColor primary,
        ConsoleColor accent,
        ConsoleColor background,
        ConsoleColor highlightForeground,
        ConsoleColor highlightBackground)
    {
        _themePrimary = primary;
        _themeAccent = accent;
        _themeBackground = background;
        _themeHighlightForeground = highlightForeground;
        _themeHighlightBackground = highlightBackground;
    }
}

// Was macht diese Datei?
// - Zeichnet Admin-Header und System-Infobereich.
// - Liest (unter Windows) CPU/RAM-Daten und aktualisiert sie regelmäßig.
