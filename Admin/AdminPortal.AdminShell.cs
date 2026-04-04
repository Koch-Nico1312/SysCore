using System.Diagnostics;
using System.Globalization;

namespace AdminApp;

public sealed partial class AdminPortal
{
    private PerformanceCounter? _adminMenueCpu;
    private PerformanceCounter? _adminMenueRam;
    private long _adminMenueLetzteSystemleisteMs;

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

    private static int AdminSystemleisteErsteZeileBerechnen(int fensterHoehe)
    {
        if (fensterHoehe <= 0)
            return 18;
        int z = (fensterHoehe * 3) / 4;
        if (z >= fensterHoehe)
            z = fensterHoehe - 2;
        return Math.Max(0, z);
    }

    private void AdminHauptmenueAsciiBannerZeichnen(int startZeile, int fensterBreite)
    {
        ConsoleColor[] farben =
        [
            ConsoleColor.Magenta, ConsoleColor.Cyan, ConsoleColor.Yellow,
            ConsoleColor.Green, ConsoleColor.Red, ConsoleColor.DarkCyan
        ];
        int h = Console.WindowHeight;
        if (h <= 0) h = 25;
        for (int i = 0; i < AdminHauptmenueAsciiBanner.Length; i++)
        {
            string zeile = AdminHauptmenueAsciiBanner[i];
            int row = startZeile + i;
            if (row < 0 || row >= h)
                continue;
            int dw = ZeichenbreiteSchaetzen(zeile);
            int pad = Math.Max(0, (fensterBreite - dw) / 2);
            Console.SetCursorPosition(0, row);
            Console.Write(new string(' ', fensterBreite));
            Console.SetCursorPosition(pad, row);
            Console.ForegroundColor = farben[i % farben.Length];
            Console.Write(zeile);
        }

        Console.ResetColor();
    }

    private void AdminMenueSystemleisteZeichnen(int fensterBreite, int fensterHoehe)
    {
        int r0 = AdminSystemleisteErsteZeileBerechnen(fensterHoehe);
        int r1 = r0 + 1;
        AdminMenueSystemzeileLeeren(fensterBreite, r0);
        if (r1 < fensterHoehe)
            AdminMenueSystemzeileLeeren(fensterBreite, r1);

        string zeile1 = AdminMenueSystemtextZeile1Erzeugen();
        string zeile2 = AdminMenueSystemtextZeile2Erzeugen();
        AdminMenueSystemzeileSchreiben(fensterBreite, r0, zeile1);
        if (r1 < fensterHoehe)
            AdminMenueSystemzeileSchreiben(fensterBreite, r1, zeile2);
    }

    private void AdminMenueSystemleisteAktualisieren(int fensterBreite, int fensterHoehe)
    {
        long jetzt = Environment.TickCount64;
        if (jetzt - _adminMenueLetzteSystemleisteMs < 550)
            return;
        _adminMenueLetzteSystemleisteMs = jetzt;

        int r0 = AdminSystemleisteErsteZeileBerechnen(fensterHoehe);
        int r1 = r0 + 1;
        string zeile1 = AdminMenueSystemtextZeile1Erzeugen();
        string zeile2 = AdminMenueSystemtextZeile2Erzeugen();
        AdminMenueSystemzeileSchreiben(fensterBreite, r0, zeile1);
        if (r1 < fensterHoehe)
            AdminMenueSystemzeileSchreiben(fensterBreite, r1, zeile2);
    }

    private static void AdminMenueSystemzeileLeeren(int fensterBreite, int zeile)
    {
        if (zeile < 0 || zeile >= Console.WindowHeight)
            return;
        Console.SetCursorPosition(0, zeile);
        Console.Write(new string(' ', fensterBreite));
    }

    private static void AdminMenueSystemzeileSchreiben(int fensterBreite, int zeile, string text)
    {
        if (zeile < 0 || zeile >= Console.WindowHeight)
            return;
        if (text.Length > fensterBreite)
            text = text[..fensterBreite];
        Console.SetCursorPosition(0, zeile);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(text.PadRight(fensterBreite));
        Console.ResetColor();
    }

    private string AdminMenueSystemtextZeile1Erzeugen()
    {
        if (OperatingSystem.IsWindows() && _adminMenueCpu != null && _adminMenueRam != null)
        {
            float cpu = _adminMenueCpu.NextValue();
            float ramMb = _adminMenueRam.NextValue();
            return string.Format(CultureInfo.InvariantCulture,
                "CPU gesamt: {0:0.#} %   RAM frei: {1:0} MB   {2}",
                cpu, ramMb, DateTime.Now.ToString("HH:mm:ss", CultureInfo.CurrentCulture));
        }

        return AdminMenueSystemtextFallbackZeile1();
    }

    private static string AdminMenueSystemtextFallbackZeile1()
    {
        Process p = Process.GetCurrentProcess();
        long mb = p.WorkingSet64 / (1024 * 1024);
        return string.Format(CultureInfo.CurrentCulture,
            "SysCore RAM: {0} MB   PID: {1}   {2}",
            mb, p.Id, DateTime.Now.ToString("HH:mm:ss", CultureInfo.CurrentCulture));
    }

    private static string AdminMenueSystemtextZeile2Erzeugen()
    {
        string name = Environment.MachineName;
        string os = Environment.OSVersion.Platform.ToString();
        string bit = Environment.Is64BitOperatingSystem ? "64-Bit" : "32-Bit";
        return string.Format(CultureInfo.CurrentCulture, "Rechner: {0}   OS: {1}   {2}", name, os, bit);
    }

    private void AdminMenueLeistenzaehlerInitialisieren()
    {
        AdminMenueLeistenzaehlerFreigeben();
        if (!OperatingSystem.IsWindows())
            return;

        _adminMenueCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _adminMenueRam = new PerformanceCounter("Memory", "Available MBytes");
        _ = _adminMenueCpu.NextValue();
        Thread.Sleep(250);
    }

    private void AdminMenueLeistenzaehlerFreigeben()
    {
        _adminMenueCpu?.Dispose();
        _adminMenueRam?.Dispose();
        _adminMenueCpu = null;
        _adminMenueRam = null;
    }

    private static bool AdminAsciiBannerPasstInBreite(int fensterBreite)
    {
        int max = 0;
        foreach (string z in AdminHauptmenueAsciiBanner)
        {
            int w = ZeichenbreiteSchaetzen(z);
            if (w > max) max = w;
        }

        return max <= fensterBreite - 2;
    }
}
