using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;
using System.Text;
using NAudio.Wave;

namespace AdminApp;

public sealed partial class AdminPortal
{
    private void SystemMonitorNachPlattformStarten()
    {
        Console.WriteLine("=== System Monitor ===");
        if (!OperatingSystem.IsWindows())
        {
            Console.WriteLine("Nur unter Windows mit Leistungsindikatoren verfügbar.");
            return;
        }

        SystemMonitorWindowsAusfuehren();
    }

    [SupportedOSPlatform("windows")]
    private void SystemMonitorWindowsAusfuehren()
    {
        using PerformanceCounter cpu = new("Processor", "% Processor Time", "_Total");
        using PerformanceCounter ram = new("Memory", "Available MBytes");
        SystemMonitorErstenCpuWertLesen(cpu);
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
            SystemMonitorZeileSchreiben(c, freiMb);
            Thread.Sleep(700);
        }
    }

    [SupportedOSPlatform("windows")]
    private static void SystemMonitorErstenCpuWertLesen(PerformanceCounter cpu)
    {
        _ = cpu.NextValue();
        Thread.Sleep(300);
    }

    private static void SystemMonitorZeileSchreiben(float cpuProzent, float ramFreiMb)
    {
        string zeile = string.Format(CultureInfo.InvariantCulture, "CPU (gesamt): {0:0.#} %   RAM frei: {1:0} MB", cpuProzent, ramFreiMb);
        Console.Write("\r" + zeile.PadRight(Console.WindowWidth > 0 ? Console.WindowWidth - 1 : 60));
    }

    private void AsciiArtGeneratorAusfuehren()
    {
        Console.WriteLine("=== ASCII Art ===");
        Console.Write("Text (A-Z, 0-9, Leerzeichen): ");
        string? t = Console.ReadLine();
        string text = t ?? "";
        if (text.Length == 0)
            return;
        string[] zeilen = AsciiArtZeilenFuerTextErzeugen(text.ToUpperInvariant());
        foreach (string z in zeilen)
            Console.WriteLine(z);
    }

    private static string[] AsciiArtZeilenFuerTextErzeugen(string text)
    {
        const int hoehe = 5;
        string[] ausgabe = new string[hoehe];
        for (int r = 0; r < hoehe; r++)
        {
            StringBuilder zeile = new();
            foreach (char c in text)
            {
                string[] muster = AsciiArtMusterFuerZeichen(c);
                if (r < muster.Length)
                    zeile.Append(muster[r]).Append(' ');
            }

            ausgabe[r] = zeile.ToString().TrimEnd();
        }

        return ausgabe;
    }

    private static string[] AsciiArtMusterFuerZeichen(char c)
    {
        if (c == ' ')
            return ["  ", "  ", "  ", "  ", "  "];

        if (c is >= '0' and <= '9')
            return AsciiArtZifferMuster(c);

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

    private static string[] AsciiArtZifferMuster(char c)
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

    private void FarbpalettenGeneratorAusfuehren()
    {
        Console.WriteLine("=== Farbpaletten ===");
        Random r = new();
        for (int i = 0; i < 5; i++)
        {
            double basis = r.NextDouble() * 360.0;
            FarbpaletteFuenfToeneDrucken(basis);
            Console.WriteLine();
        }
    }

    private static void FarbpaletteFuenfToeneDrucken(double startHue)
    {
        for (int i = 0; i < 5; i++)
        {
            double h = (startHue + i * 18.0) % 360.0;
            FarbeAusHsvNachKonsole(h, 0.75, 0.72);
        }
    }

    private static void FarbeAusHsvNachKonsole(double h, double s, double v)
    {
        HsvZuRgbUmrechnen(h, s, v, out int rr, out int gg, out int bb);
        string hex = string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", rr, gg, bb);
        ConsoleColor vordergrund = FarbeConsoleAmNaechstenWaehlen(rr, gg, bb);
        Console.ForegroundColor = vordergrund;
        Console.Write(hex + "  ");
        Console.ResetColor();
    }

    private static void HsvZuRgbUmrechnen(double h, double s, double v, out int r, out int g, out int b)
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

    private static int ClampByte(double wert)
    {
        if (wert < 0)
            return 0;
        if (wert > 255)
            return 255;
        return (int)Math.Round(wert, MidpointRounding.AwayFromZero);
    }

    private static ConsoleColor FarbeConsoleAmNaechstenWaehlen(int r, int g, int b)
    {
        int grau = (r + g + b) / 3;
        if (grau < 85)
            return ConsoleColor.DarkGray;
        if (grau > 170)
            return ConsoleColor.White;

        if (r > g && r > b)
            return r > 200 ? ConsoleColor.Red : ConsoleColor.DarkRed;
        if (g > r && g > b)
            return g > 200 ? ConsoleColor.Green : ConsoleColor.DarkGreen;
        if (b > r && b > g)
            return b > 200 ? ConsoleColor.Blue : ConsoleColor.DarkBlue;

        return ConsoleColor.Gray;
    }

    private void MusikPlayerAusfuehren()
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
        MusikWiedergabeSchleife(ausgabe);
        ausgabe.Stop();
    }

    private static string MusikPfadBereinigen(string roh)
    {
        string t = roh.Trim();
        if (t.Length >= 2 && t[0] == '"' && t[^1] == '"')
            t = t[1..^1];
        return t;
    }

    private static void MusikWiedergabeSchleife(WaveOutEvent ausgabe)
    {
        while (ausgabe.PlaybackState == PlaybackState.Playing)
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
