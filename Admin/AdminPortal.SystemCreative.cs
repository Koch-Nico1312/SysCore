using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;
using System.Text;
using NAudio.Wave;

namespace AdminApp;

public sealed partial class AdminPortal
{
    // Einstiegspunkt für den Systemmonitor.
    private void RunSystemMonitorByPlatform()
    {
        Console.WriteLine("=== System Monitor ===");
        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("Nur unter Windows mit Leistungsindikatoren verfügbar.");
            return;
        }

        RunSystemMonitorWindows();
    }

    // WICHTIG:
    // Diese Methode nutzt PerformanceCounter (Windows-System-API).
    // Das ist fortgeschritten, wir behalten es für die gleiche Funktion.
    [SupportedOSPlatform("windows")]
    private void RunSystemMonitorWindows()
    {
        using PerformanceCounter cpu = new("Processor", "% Processor Time", "_Total");
        using PerformanceCounter ram = new("Memory", "Available MBytes");
        ReadFirstSystemMonitorCpuValue(cpu);
        Console.WriteLine("Live — Taste drücken zum Beenden.");
        while (true)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo k = Console.ReadKey(intercept: true);
                if (k.Key == ConsoleKey.Escape || k.Key == ConsoleKey.Q || k.Key == ConsoleKey.Enter)
                    break;
            }

            float c = cpu.NextValue();
            float freiMb = ram.NextValue();
            WriteSystemMonitorLine(c, freiMb);
            Thread.Sleep(700);
        }
    }

    // Liest einen ersten CPU-Wert, damit Folgemessungen realistischer sind.
    [SupportedOSPlatform("windows")]
    private static void ReadFirstSystemMonitorCpuValue(PerformanceCounter cpu)
    {
        _ = cpu.NextValue();
        Thread.Sleep(300);
    }

    // Schreibt eine Live-Zeile für CPU/RAM.
    private static void WriteSystemMonitorLine(float cpuPercent, float ramFreeMb)
    {
        string zeile = string.Format(CultureInfo.InvariantCulture, "CPU (gesamt): {0:0.#} %   RAM frei: {1:0} MB", cpuPercent, ramFreeMb);
        int width = 60;
        if (Console.WindowWidth > 0)
        {
            width = Console.WindowWidth - 1;
        }
        Console.Write("\r" + zeile.PadRight(width));
    }

    // ASCII-Art Generator für einfachen Text.
    private void RunAsciiArtGenerator()
    {
        Console.WriteLine("=== ASCII Art ===");
        Console.Write("Text (A-Z, 0-9, Leerzeichen): ");
        string? t = Console.ReadLine();
        string text = t ?? "";
        if (text.Length == 0)
            return;
        string[] zeilen = BuildAsciiArtLinesForText(text.ToUpperInvariant());
        foreach (string z in zeilen)
            Console.WriteLine(z);
    }

    // Baut die Ausgabezeilen für den gesamten Text.
    private static string[] BuildAsciiArtLinesForText(string text)
    {
        const int hoehe = 5;
        string[] ausgabe = new string[hoehe];
        for (int r = 0; r < hoehe; r++)
        {
            StringBuilder zeile = new();
            foreach (char c in text)
            {
                string[] muster = GetAsciiArtPatternForCharacter(c);
                if (r < muster.Length)
                    zeile.Append(muster[r]).Append(' ');
            }

            ausgabe[r] = zeile.ToString().TrimEnd();
        }

        return ausgabe;
    }

    // Gibt Muster für ein Zeichen zurück.
    private static string[] GetAsciiArtPatternForCharacter(char c)
    {
        if (c == ' ')
            return ["  ", "  ", "  ", "  ", "  "];

        if (c is >= '0' and <= '9')
            return GetAsciiArtDigitPattern(c);

        return c switch
        {
            'A' => ["  █  ", " █ █ ", "█   █", "█████", "█   █"],
            'B' => ["████ ", "█   █", "████ ", "█   █", "████ "],
            'C' => [" ███", "█   ", "█   ", "█   ", " ███"],
            'D' => ["████ ", "█   █", "█   █", "█   █", "████ "],
            'E' => ["█████", "█    ", "████ ", "█    ", "█████"],
            'F' => ["█████", "█    ", "████ ", "█    ", "█    "],
            'G' => [" ███", "█    ", "█  ██", "█   █", " ███"],
            'H' => ["█   █", "█   █", "█████", "█   █", "█   █"],
            'I' => ["█████", "  █  ", "  █  ", "  █  ", "█████"],
            'J' => ["  ███", "   █ ", "   █ ", "█  █ ", " ██  "],
            'K' => ["█   █", "█  █ ", "███  ", "█  █ ", "█   █"],
            'L' => ["█    ", "█    ", "█    ", "█    ", "█████"],
            'M' => ["█   █", "██ ██", "█ █ █", "█   █", "█   █"],
            'N' => ["█   █", "██  █", "█ █ █", "█  ██", "█   █"],
            'O' => [" ███ ", "█   █", "█   █", "█   █", " ███ "],
            'P' => ["████ ", "█   █", "████ ", "█    ", "█    "],
            'Q' => [" ███ ", "█   █", "█   █", "█  █ ", " ██ █"],
            'R' => ["████ ", "█   █", "████ ", "█  █ ", "█   █"],
            'S' => [" ████", "█    ", " ███ ", "    █", "████ "],
            'T' => ["█████", "  █  ", "  █  ", "  █  ", "  █  "],
            'U' => ["█   █", "█   █", "█   █", "█   █", " ███ "],
            'V' => ["█   █", "█   █", "█   █", " █ █ ", "  █  "],
            'W' => ["█   █", "█   █", "█ █ █", "██ ██", "█   █"],
            'X' => ["█   █", " █ █ ", "  █  ", " █ █ ", "█   █"],
            'Y' => ["█   █", " █ █ ", "  █  ", "  █  ", "  █  "],
            'Z' => ["█████", "   █ ", "  █  ", " █   ", "█████"],
            _ => ["?", "?", "?", "?", "?"]
        };
    }

    // Gibt Muster für Ziffern zurück.
    private static string[] GetAsciiArtDigitPattern(char c)
    {
        return c switch
        {
            '0' => [" ███ ", "█  ██", "█ █ █", "██  █", " ███ "],
            '1' => ["  █  ", " ██  ", "  █  ", "  █  ", "█████"],
            '2' => ["████ ", "    █", " ███ ", "█    ", "█████"],
            '3' => ["████ ", "    █", " ███ ", "    █", "████ "],
            '4' => ["█   █", "█   █", "█████", "    █", "    █"],
            '5' => ["█████", "█    ", "████ ", "    █", "████ "],
            '6' => [" ███ ", "█    ", "████ ", "█   █", " ███ "],
            '7' => ["█████", "    █", "   █ ", "  █  ", " █   "],
            '8' => [" ███ ", "█   █", " ███ ", "█   █", " ███ "],
            '9' => [" ███ ", "█   █", " ████", "    █", " ███ "],
            _ => ["?", "?", "?", "?", "?"]
        };
    }

    // Zeigt ein eigenes Auswahlmenü und übernimmt das Theme ins Admin-Layout.
    private void RunColorPaletteGenerator()
    {
        if (!Console.IsOutputRedirected)
        {
            Console.Clear();
            Console.ResetColor();
        }

        Console.WriteLine("=== Color Palette ===");
        Console.WriteLine();
        Console.WriteLine("1) Midnight Noir      BG #000000  FG #808000  ACC #808080");
        Console.WriteLine("2) Neon Tokyo         BG #800080  FG #FFFFFF  ACC #00FFFF");
        Console.WriteLine("3) Forest Dusk        BG #000000  FG #008000  ACC #008000");
        Console.WriteLine("4) Arctic Frost       BG #008080  FG #FFFFFF  ACC #0000FF");
        Console.WriteLine("5) Lava Flow          BG #800000  FG #FFFF00  ACC #FFFF00");
        Console.WriteLine("6) Nur Akzentfarbe waehlen");
        Console.WriteLine();
        Console.Write("Choose (1-6): ");
        string? input = Console.ReadLine();
        string choice = input?.Trim() ?? "";

        switch (choice)
        {
            case "1":
                SetAdminTheme(
                    primary: ConsoleColor.DarkYellow,
                    accent: ConsoleColor.DarkGray,
                    background: ConsoleColor.Black,
                    highlightForeground: ConsoleColor.White,
                    highlightBackground: ConsoleColor.DarkYellow);
                break;
            case "2":
                SetAdminTheme(
                    primary: ConsoleColor.White,
                    accent: ConsoleColor.Cyan,
                    background: ConsoleColor.DarkMagenta,
                    highlightForeground: ConsoleColor.White,
                    highlightBackground: ConsoleColor.DarkBlue);
                break;
            case "3":
                SetAdminTheme(
                    primary: ConsoleColor.Green,
                    accent: ConsoleColor.DarkGreen,
                    background: ConsoleColor.Black,
                    highlightForeground: ConsoleColor.White,
                    highlightBackground: ConsoleColor.DarkGreen);
                break;
            case "4":
                SetAdminTheme(
                    primary: ConsoleColor.White,
                    accent: ConsoleColor.Blue,
                    background: ConsoleColor.DarkCyan,
                    highlightForeground: ConsoleColor.White,
                    highlightBackground: ConsoleColor.Blue);
                break;
            case "5":
                SetAdminTheme(
                    primary: ConsoleColor.Yellow,
                    accent: ConsoleColor.Yellow,
                    background: ConsoleColor.DarkRed,
                    highlightForeground: ConsoleColor.White,
                    highlightBackground: ConsoleColor.Red);
                break;
            case "6":
                RunAdminAccentColorPicker();
                break;
            default:
                return;
        }
    }

    private void RunAdminAccentColorPicker()
    {
        Console.WriteLine();
        Console.WriteLine("Akzentfarbe:");
        Console.WriteLine("1) Cyan  2) Green  3) Magenta  4) Yellow  5) Blue  6) White");
        Console.Write("> ");
        string? w = Console.ReadLine();
        ConsoleColor accent = w?.Trim() switch
        {
            "1" => ConsoleColor.Cyan,
            "2" => ConsoleColor.Green,
            "3" => ConsoleColor.Magenta,
            "4" => ConsoleColor.Yellow,
            "5" => ConsoleColor.Blue,
            "6" => ConsoleColor.White,
            _ => _themeAccent
        };

        SetAdminTheme(
            primary: _themePrimary,
            accent: accent,
            background: _themeBackground,
            highlightForeground: _themeHighlightForeground,
            highlightBackground: _themeHighlightBackground);
    }

    // Schreibt einen HEX-Farbwert eingefärbt in die Konsole.
    private static void WritePaletteHexColor(string hex)
    {
        if (hex.Length == 7 && hex[0] == '#'
            && int.TryParse(hex.AsSpan(1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int r)
            && int.TryParse(hex.AsSpan(3, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int g)
            && int.TryParse(hex.AsSpan(5, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int b))
        {
            Console.ForegroundColor = PickClosestConsoleColor(r, g, b);
            Console.WriteLine(hex);
            Console.ResetColor();
            return;
        }

        Console.WriteLine(hex);
    }

    // Rechnet HSV in RGB um und schreibt die Hex-Farbe.
    private static void WriteConsoleColorFromHsv(double h, double s, double v)
    {
        ConvertHsvToRgb(h, s, v, out int rr, out int gg, out int bb);
        string hex = string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", rr, gg, bb);
        ConsoleColor vordergrund = PickClosestConsoleColor(rr, gg, bb);
        Console.ForegroundColor = vordergrund;
        Console.Write(hex + "  ");
        Console.ResetColor();
    }

    // Mathematische Umrechnung HSV -> RGB.
    private static void ConvertHsvToRgb(double h, double s, double v, out int r, out int g, out int b)
    {
        double c = v * s;
        double x = c * (1 - Math.Abs(h / 60.0 % 2 - 1));
        double m = v - c;
        double rp = 0, gp = 0, bp = 0;
        if (h < 60)
        {
            rp = c;
            gp = x;
        }
        else if (h < 120)
        {
            rp = x;
            gp = c;
        }
        else if (h < 180)
        {
            gp = c;
            bp = x;
        }
        else if (h < 240)
        {
            gp = x;
            bp = c;
        }
        else if (h < 300)
        {
            rp = x;
            bp = c;
        }
        else
        {
            rp = c;
            bp = x;
        }

        r = ClampByte((rp + m) * 255.0);
        g = ClampByte((gp + m) * 255.0);
        b = ClampByte((bp + m) * 255.0);
    }

    // Begrenzung auf Byte-Bereich 0..255.
    private static int ClampByte(double value)
    {
        if (value < 0)
            return 0;
        if (value > 255)
            return 255;
        return (int)Math.Round(value, MidpointRounding.AwayFromZero);
    }

    // Wählt eine ungefähr passende Konsolenfarbe.
    private static ConsoleColor PickClosestConsoleColor(int r, int g, int b)
    {
        int grau = (r + g + b) / 3;
        if (grau < 85)
            return ConsoleColor.DarkGray;
        if (grau > 170)
            return ConsoleColor.White;

        if (r > g && r > b)
        {
            if (r > 200)
            {
                return ConsoleColor.Red;
            }
            return ConsoleColor.DarkRed;
        }
        if (g > r && g > b)
        {
            if (g > 200)
            {
                return ConsoleColor.Green;
            }
            return ConsoleColor.DarkGreen;
        }
        if (b > r && b > g)
        {
            if (b > 200)
            {
                return ConsoleColor.Blue;
            }
            return ConsoleColor.DarkBlue;
        }

        return ConsoleColor.Gray;
    }

    // Spielt eine MP3-Datei ab.
    // Hinweis: NAudio ist eine externe Bibliothek für Audio.
    private void RunMusicPlayer()
    {
        Console.WriteLine("=== Musik Player (MP3) ===");
        Console.Write("Pfad zur MP3: ");
        string? p = Console.ReadLine();
        string roh = p ?? "";
        string pfad = MusikPfadBereinigen(roh);
        if (!File.Exists(pfad))
        {
            Console.WriteLine("Datei nicht gefunden.");
            return;
        }

        using WaveOutEvent ausgabe = new();
        using AudioFileReader reader = new(pfad);
        ausgabe.Init(reader);
        ausgabe.Play();
        Console.WriteLine("Wiedergabe — Taste zum Stoppen.");
        RunMusicPlaybackLoop(ausgabe);
        ausgabe.Stop();
    }

    // Entfernt unnötige Anführungszeichen am Pfad.
    private static string MusikPfadBereinigen(string roh)
    {
        string t = roh.Trim();
        if (t.Length >= 2 && t[0] == '"' && t[^1] == '"')
            t = t[1..^1];
        return t;
    }

    // Wartet auf Tastendruck zum Stoppen der Wiedergabe.
    private static void RunMusicPlaybackLoop(WaveOutEvent output)
    {
        while (output.PlaybackState == PlaybackState.Playing)
        {
            if (Console.KeyAvailable)
            {
                Console.ReadKey(intercept: true);
                break;
            }

            Thread.Sleep(50);
        }
    }
}

// Was macht diese Datei?
// - Enthält kreative/systemnahe Tools: Systemmonitor, ASCII-Art, Farbpaletten, Musikplayer.
// - Nutzt teils fortgeschrittene APIs (PerformanceCounter/NAudio), damit die Features funktionieren.
