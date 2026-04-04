namespace AdminApp;

public sealed partial class AdminPortal
{
    private int _menueStartZeile;
    private int _letzteFensterBreite;
    private int _letzteFensterHoehe;

    private int ZeigeHauptmenueUndWaehle()
    {
        return FuehreMenueAus("Admin — Hauptmenü", HauptmenuePunkte, indexBeiEscape: 1, adminHauptmenueAscii: true);
    }

    private int ZeigeProgrammListeUndWaehle()
    {
        string[] alle = new string[AdminProgrammEintraege.Length + 1];
        for (int i = 0; i < AdminProgrammEintraege.Length; i++)
            alle[i] = AdminProgrammEintraege[i];
        alle[^1] = "<< Zurück";
        int gewaehlt = FuehreMenueAus("Programme starten", alle, indexBeiEscape: alle.Length - 1, adminHauptmenueAscii: false);
        if (gewaehlt == alle.Length - 1)
            return -1;
        return gewaehlt;
    }

    private int FuehreMenueAus(string titel, IReadOnlyList<string> zeilen, int indexBeiEscape, bool adminHauptmenueAscii)
    {
        int markiert = 0;
        bool neuZeichnen = true;

        while (true)
        {
            if (neuZeichnen)
            {
                MenueZeichnen(titel, zeilen, markiert, adminHauptmenueAscii);
                neuZeichnen = false;
            }
            else
            {
                int w = Console.WindowWidth;
                int h = Console.WindowHeight;
                if (w <= 0) w = 80;
                if (h <= 0) h = 25;
                AdminMenueSystemleisteAktualisieren(w, h);
            }

            if (OperatingSystem.IsWindows() && _consoleInputHandle != nint.Zero && _consoleInputHandle != new nint(-1))
            {
                ConsoleInputWindows.WaitForInput(_consoleInputHandle, 40);
                while (ConsoleInputWindows.TryReadOneInput(_consoleInputHandle, out ConsoleInputWindows.InputRecord rec, out bool hat) && hat)
                {
                    if (rec.EventType == ConsoleInputWindows.WindowBufferSizeEvent)
                    {
                        neuZeichnen = true;
                        continue;
                    }

                    if (rec.EventType == ConsoleInputWindows.MouseEvent)
                    {
                        int? aktiv = VerarbeiteMausFuerMenue(rec.MouseEvent, zeilen.Count, ref markiert, ref neuZeichnen);
                        if (aktiv.HasValue)
                            return aktiv.Value;
                    }

                    if (rec.EventType == ConsoleInputWindows.KeyEvent)
                    {
                        if (rec.KeyEvent.KeyDown == 0)
                            continue;
                        int? ergebnis = VerarbeiteTasteFuerMenue(rec.KeyEvent, zeilen.Count, ref markiert, indexBeiEscape);
                        if (ergebnis.HasValue)
                            return ergebnis.Value;
                        neuZeichnen = true;
                    }
                }

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo k = Console.ReadKey(intercept: true);
                    int? ergebnis = VerarbeiteConsoleKeyInfo(k, zeilen.Count, ref markiert, indexBeiEscape);
                    if (ergebnis.HasValue)
                        return ergebnis.Value;
                    neuZeichnen = true;
                }
            }
            else
            {
                ConsoleKeyInfo k = Console.ReadKey(intercept: true);
                int? ergebnis = VerarbeiteConsoleKeyInfo(k, zeilen.Count, ref markiert, indexBeiEscape);
                if (ergebnis.HasValue)
                    return ergebnis.Value;
                neuZeichnen = true;
            }
        }
    }

    private void MenueZeichnen(string titel, IReadOnlyList<string> zeilen, int markiert, bool adminHauptmenueAscii)
    {
        int w = Console.WindowWidth;
        int h = Console.WindowHeight;
        if (w <= 0) w = 80;
        if (h <= 0) h = 25;
        _letzteFensterBreite = w;
        _letzteFensterHoehe = h;

        Console.Clear();
        Console.ResetColor();
        Console.CursorVisible = false;

        int ersteZeileNachKopf;
        const int kopfStartZeile = 1;
        if (adminHauptmenueAscii)
        {
            if (AdminAsciiBannerPasstInBreite(w))
            {
                AdminHauptmenueAsciiBannerZeichnen(kopfStartZeile, w);
                ersteZeileNachKopf = kopfStartZeile + AdminHauptmenueAsciiBanner.Length + 1;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                SchreibeZentriert("═══  ADMIN  ═══", kopfStartZeile, w);
                Console.ResetColor();
                ersteZeileNachKopf = 3;
            }

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            SchreibeZentriert(titel, ersteZeileNachKopf - 1, w);
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            SchreibeZentriert(titel, 1, w);
            Console.ResetColor();
            ersteZeileNachKopf = 3;
        }

        int infoZeile = AdminSystemleisteErsteZeileBerechnen(h);
        int letzteErlaubteMenueZeile = infoZeile - 2;
        if (letzteErlaubteMenueZeile < ersteZeileNachKopf)
            letzteErlaubteMenueZeile = Math.Max(ersteZeileNachKopf, infoZeile - 1);

        int spannweite = letzteErlaubteMenueZeile - ersteZeileNachKopf + 1;
        int start = ersteZeileNachKopf;
        if (spannweite >= zeilen.Count)
            start = ersteZeileNachKopf + (spannweite - zeilen.Count) / 2;
        if (start + zeilen.Count - 1 > letzteErlaubteMenueZeile)
            start = Math.Max(ersteZeileNachKopf, letzteErlaubteMenueZeile - zeilen.Count + 1);

        _menueStartZeile = start;

        for (int i = 0; i < zeilen.Count; i++)
        {
            string zeile = zeilen[i];
            bool aktiv = i == markiert;
            int zeileY = start + i;
            if (zeileY < 0 || zeileY >= h)
                continue;

            Console.SetCursorPosition(0, zeileY);
            Console.Write(new string(' ', w));

            int dw = ZeichenbreiteSchaetzen(zeile);
            int pad = Math.Max(0, (w - dw) / 2);
            Console.SetCursorPosition(pad, zeileY);

            if (aktiv)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            Console.Write(zeile);
            Console.ResetColor();
        }

        AdminMenueSystemleisteZeichnen(w, h);
        _adminMenueLetzteSystemleisteMs = Environment.TickCount64;
    }

    private static void SchreibeZentriert(string text, int zeile, int fensterBreite)
    {
        if (zeile < 0 || zeile >= Console.WindowHeight)
            return;
        int dw = ZeichenbreiteSchaetzen(text);
        int pad = Math.Max(0, (fensterBreite - dw) / 2);
        Console.SetCursorPosition(0, zeile);
        Console.Write(new string(' ', fensterBreite));
        Console.SetCursorPosition(pad, zeile);
        Console.Write(text);
    }

    private static int ZeichenbreiteSchaetzen(string s)
    {
        int breite = 0;
        foreach (var r in s.EnumerateRunes())
        {
            breite += r.Value is >= 0x1100 and <= 0x115F
                or >= 0x2329 and <= 0x232A
                or >= 0x2E80 and <= 0xA4CF
                or >= 0xAC00 and <= 0xD7A3
                or >= 0xF900 and <= 0xFAFF
                or >= 0xFE10 and <= 0xFE19
                or >= 0xFE30 and <= 0xFE6F
                or >= 0xFF00 and <= 0xFF60
                or >= 0xFFE0 and <= 0xFFE6
                ? 2 : 1;
        }

        return breite;
    }

    private int? VerarbeiteMausFuerMenue(ConsoleInputWindows.MouseEventRecord maus, int anzahlZeilen, ref int markiert, ref bool neuZeichnen)
    {
        short y = maus.MousePosition.Y;
        if (y < 0)
            return null;

        int index = y - _menueStartZeile;
        bool inZeile = index >= 0 && index < anzahlZeilen;

        if ((maus.EventFlags & ConsoleInputWindows.MouseMoved) != 0 && maus.ButtonState == 0)
        {
            if (inZeile && index != markiert)
            {
                markiert = index;
                neuZeichnen = true;
            }

            return null;
        }

        if (maus.EventFlags != 0)
            return null;

        if ((maus.ButtonState & ConsoleInputWindows.FromLeft1stButtonPressed) == 0)
            return null;

        if (!inZeile)
            return null;

        return index;
    }

    private static int? VerarbeiteTasteFuerMenue(ConsoleInputWindows.KeyEventRecord taste, int anzahlZeilen, ref int markiert, int indexBeiEscape)
    {
        ushort vk = taste.VirtualKeyCode;
        if (vk == 0x1B)
            return indexBeiEscape;
        if (vk == 0x0D)
            return markiert;
        if (vk == 0x26)
        {
            markiert = markiert <= 0 ? anzahlZeilen - 1 : markiert - 1;
            return null;
        }

        if (vk == 0x28)
        {
            markiert = markiert >= anzahlZeilen - 1 ? 0 : markiert + 1;
            return null;
        }

        return null;
    }

    private static int? VerarbeiteConsoleKeyInfo(ConsoleKeyInfo k, int anzahlZeilen, ref int markiert, int indexBeiEscape)
    {
        if (k.Key == ConsoleKey.Escape)
            return indexBeiEscape;
        if (k.Key == ConsoleKey.Enter)
            return markiert;
        if (k.Key == ConsoleKey.UpArrow)
        {
            markiert = markiert <= 0 ? anzahlZeilen - 1 : markiert - 1;
            return null;
        }

        if (k.Key == ConsoleKey.DownArrow)
        {
            markiert = markiert >= anzahlZeilen - 1 ? 0 : markiert + 1;
            return null;
        }

        return null;
    }
}
