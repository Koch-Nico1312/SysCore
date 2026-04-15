namespace AdminApp;

public sealed partial class AdminPortal
{
    private int _menuStartRow;
    private int _lastWindowWidth;
    private int _lastWindowHeight;

    // Zeigt das Hauptmenü und gibt den gewählten Index zurück.
    private int ShowMainMenuAndSelect()
    {
        return RunMenu("Admin — Hauptmenü", MainMenuItems, indexOnEscape: 6, isAdminMainMenuAscii: true);
    }

    // Baut die Programmliste plus "Zurück" und gibt die Auswahl zurück.
    private int ShowProgramListAndSelect()
    {
        string[] entries = new string[AdminProgramEntries.Length + 1];
        for (int i = 0; i < AdminProgramEntries.Length; i++)
            entries[i] = AdminProgramEntries[i];
        entries[entries.Length - 1] = "<< Zurück";
        int selected = RunMenu("Programme starten", entries, indexOnEscape: entries.Length - 1, isAdminMainMenuAscii: true);
        if (selected == entries.Length - 1)
            return -1;
        return selected;
    }

    // Zentrale Menü-Logik: zeichnet, liest Eingaben, liefert Auswahl.
    private int RunMenu(string title, IReadOnlyList<string> lines, int indexOnEscape, bool isAdminMainMenuAscii)
    {
        int selectedIndex = 0;
        bool redraw = true;

        while (true)
        {
            if (redraw)
            {
                DrawMenu(title, lines, selectedIndex, isAdminMainMenuAscii);
                redraw = false;
            }
            else
            {
                if (isAdminMainMenuAscii)
                {
                    UpdateAdminRetroTimeAndDate(immediate: false);
                }
                else
                {
                    int w = Console.WindowWidth;
                    int h = Console.WindowHeight;
                    if (w <= 0) w = 80;
                    if (h <= 0) h = 25;
                    UpdateAdminMenuSystemBar(w, h);
                }
            }

            if (OperatingSystem.IsWindows() && _consoleInputHandle != nint.Zero && _consoleInputHandle != new nint(-1))
            {
                ConsoleInputWindows.WaitForInput(_consoleInputHandle, 40);
                while (ConsoleInputWindows.TryReadOneInput(_consoleInputHandle, out ConsoleInputWindows.InputRecord rec, out bool hat) && hat)
                {
                    if (rec.EventType == ConsoleInputWindows.WindowBufferSizeEvent)
                    {
                        redraw = true;
                        continue;
                    }

                    if (rec.EventType == ConsoleInputWindows.MouseEvent)
                    {
                        int? active = HandleMouseForMenu(rec.MouseEvent, lines.Count, ref selectedIndex, ref redraw);
                        if (active.HasValue)
                            return active.Value;
                    }

                    if (rec.EventType == ConsoleInputWindows.KeyEvent)
                    {
                        if (rec.KeyEvent.KeyDown == 0)
                            continue;
                        if (isAdminMainMenuAscii && HandleAdminRetroHotkey(rec.KeyEvent))
                            continue;
                        int? result = HandleKeyForMenu(rec.KeyEvent, lines.Count, ref selectedIndex, indexOnEscape);
                        if (result.HasValue)
                            return result.Value;
                        redraw = true;
                    }
                }

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo k = Console.ReadKey(intercept: true);
                    if (isAdminMainMenuAscii && HandleAdminRetroHotkey(k))
                        continue;
                    int? result = HandleConsoleKeyInfo(k, lines.Count, ref selectedIndex, indexOnEscape);
                    if (result.HasValue)
                        return result.Value;
                    redraw = true;
                }
            }
            else
            {
                ConsoleKeyInfo k = Console.ReadKey(intercept: true);
                if (isAdminMainMenuAscii && HandleAdminRetroHotkey(k))
                    continue;
                int? result = HandleConsoleKeyInfo(k, lines.Count, ref selectedIndex, indexOnEscape);
                if (result.HasValue)
                    return result.Value;
                redraw = true;
            }
        }
    }

    // Zeichnet den kompletten Menü-Bildschirm neu.
    private void DrawMenu(string title, IReadOnlyList<string> lines, int selectedIndex, bool isAdminMainMenuAscii)
    {
        int w = Console.WindowWidth;
        int h = Console.WindowHeight;
        if (w <= 0) w = 80;
        if (h <= 0) h = 25;
        _lastWindowWidth = w;
        _lastWindowHeight = h;

        Console.Clear();
        Console.ResetColor();
        Console.CursorVisible = false;

        if (isAdminMainMenuAscii)
        {
            DrawAdminRetroLayout(lines, selectedIndex);
            _adminMenuLastSystemBarMs = Environment.TickCount64;
            return;
        }

        int ersteZeileNachKopf;
        Console.ForegroundColor = ConsoleColor.Cyan;
        WriteCentered(title, 1, w);
        Console.ResetColor();
        ersteZeileNachKopf = 3;

        int infoLine = CalculateAdminSystemBarFirstLine(h);
        int lastAllowedMenuLine = infoLine - 2;
        if (lastAllowedMenuLine < ersteZeileNachKopf)
            lastAllowedMenuLine = Math.Max(ersteZeileNachKopf, infoLine - 1);

        int span = lastAllowedMenuLine - ersteZeileNachKopf + 1;
        int start = ersteZeileNachKopf;
        if (span >= lines.Count)
            start = ersteZeileNachKopf + (span - lines.Count) / 2;
        if (start + lines.Count - 1 > lastAllowedMenuLine)
            start = Math.Max(ersteZeileNachKopf, lastAllowedMenuLine - lines.Count + 1);

        _menuStartRow = start;

        for (int i = 0; i < lines.Count; i++)
        {
            string zeile = lines[i];
            bool aktiv = i == selectedIndex;
            int zeileY = start + i;
            if (zeileY < 0 || zeileY >= h)
                continue;

            Console.SetCursorPosition(0, zeileY);
            Console.Write(new string(' ', w));

            int dw = EstimateDisplayWidth(zeile);
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

        DrawAdminMenuSystemBar(w, h);
        _adminMenuLastSystemBarMs = Environment.TickCount64;
    }

    // Sondertasten fuer das Retro-Hauptmenue: Lautstaerke und Song-Text.
    private bool HandleAdminRetroHotkey(ConsoleInputWindows.KeyEventRecord key)
    {
        ushort vk = key.VirtualKeyCode;
        char c = (char)key.UnicodeChar;

        if (vk == 0x6B || c == '+')
        {
            ChangeAdminRetroVolume(5);
            return true;
        }

        if (vk == 0x6D || c == '-')
        {
            ChangeAdminRetroVolume(-5);
            return true;
        }

        if (vk == 0x4E || c == 'n' || c == 'N')
        {
            SetAdminRetroSongText("♪ Naechster Song (Demo)");
            return true;
        }

        if (vk == 0x50 || c == 'p' || c == 'P')
        {
            SetAdminRetroSongText("♪ Vorheriger Song (Demo)");
            return true;
        }

        if (vk == 0x4D || c == 'm' || c == 'M')
        {
            SetAdminRetroSongText("♪ Musik-Ordner: waehlen...");
            return true;
        }

        return false;
    }

    // Sondertasten fuer das Retro-Hauptmenue (ConsoleKeyInfo-Variante).
    private bool HandleAdminRetroHotkey(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Add || key.KeyChar == '+')
        {
            ChangeAdminRetroVolume(5);
            return true;
        }

        if (key.Key == ConsoleKey.Subtract || key.KeyChar == '-')
        {
            ChangeAdminRetroVolume(-5);
            return true;
        }

        if (key.Key == ConsoleKey.N)
        {
            SetAdminRetroSongText("♪ Naechster Song (Demo)");
            return true;
        }

        if (key.Key == ConsoleKey.P)
        {
            SetAdminRetroSongText("♪ Vorheriger Song (Demo)");
            return true;
        }

        if (key.Key == ConsoleKey.M)
        {
            SetAdminRetroSongText("♪ Musik-Ordner: waehlen...");
            return true;
        }

        return false;
    }

    // Hilfsmethode: schreibt einen Text mittig in eine Zeile.
    private static void WriteCentered(string text, int row, int windowWidth)
    {
        if (row < 0 || row >= Console.WindowHeight)
            return;
        int dw = EstimateDisplayWidth(text);
        int pad = Math.Max(0, (windowWidth - dw) / 2);
        Console.SetCursorPosition(0, row);
        Console.Write(new string(' ', windowWidth));
        Console.SetCursorPosition(pad, row);
        Console.Write(text);
    }

    // Schätzt die Breite eines Textes (inklusive breiter Unicode-Zeichen).
    // Das ist nur für saubere Ausrichtung in der Konsole.
    private static int EstimateDisplayWidth(string s)
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

    // Mausbewegung/Mausklick für Menüauswahl auswerten.
    private int? HandleMouseForMenu(ConsoleInputWindows.MouseEventRecord mouse, int lineCount, ref int selectedIndex, ref bool redraw)
    {
        short y = mouse.MousePosition.Y;
        if (y < 0)
            return null;

        int index = y - _menuStartRow;
        bool inLine = index >= 0 && index < lineCount;

        if ((mouse.EventFlags & ConsoleInputWindows.MouseMoved) != 0 && mouse.ButtonState == 0)
        {
            if (inLine && index != selectedIndex)
            {
                selectedIndex = index;
                redraw = true;
            }

            return null;
        }

        if (mouse.EventFlags != 0)
            return null;

        if ((mouse.ButtonState & ConsoleInputWindows.FromLeft1stButtonPressed) == 0)
            return null;

        if (!inLine)
            return null;

        return index;
    }

    // Tastatur-Ereignisse aus Windows-Inputrecord auswerten.
    private static int? HandleKeyForMenu(ConsoleInputWindows.KeyEventRecord key, int lineCount, ref int selectedIndex, int indexOnEscape)
    {
        ushort vk = key.VirtualKeyCode;
        if (vk == 0x1B)
            return indexOnEscape;
        if (vk == 0x0D)
            return selectedIndex;
        if (vk == 0x26)
        {
            if (selectedIndex <= 0)
            {
                selectedIndex = lineCount - 1;
            }
            else
            {
                selectedIndex = selectedIndex - 1;
            }
            return null;
        }

        if (vk == 0x28)
        {
            if (selectedIndex >= lineCount - 1)
            {
                selectedIndex = 0;
            }
            else
            {
                selectedIndex = selectedIndex + 1;
            }
            return null;
        }

        return null;
    }

    // Normale Console-Tasten (Pfeile/Enter/Escape) auswerten.
    private static int? HandleConsoleKeyInfo(ConsoleKeyInfo key, int lineCount, ref int selectedIndex, int indexOnEscape)
    {
        if (key.Key == ConsoleKey.Escape)
            return indexOnEscape;
        if (key.Key == ConsoleKey.Enter)
            return selectedIndex;
        if (key.Key == ConsoleKey.UpArrow)
        {
            if (selectedIndex <= 0)
            {
                selectedIndex = lineCount - 1;
            }
            else
            {
                selectedIndex = selectedIndex - 1;
            }
            return null;
        }

        if (key.Key == ConsoleKey.DownArrow)
        {
            if (selectedIndex >= lineCount - 1)
            {
                selectedIndex = 0;
            }
            else
            {
                selectedIndex = selectedIndex + 1;
            }
            return null;
        }

        return null;
    }
}

// Was macht diese Datei?
// - Enthält die komplette Menü-Navigation (Zeichnen + Eingabe).
// - Unterstützt Tastatur und unter Windows zusätzlich Maus-Events.
