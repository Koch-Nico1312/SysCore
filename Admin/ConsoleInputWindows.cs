using System.Runtime.InteropServices;

namespace AdminApp;

// WICHTIG FÜR DICH:
// Diese Datei kapselt Windows-API-Aufrufe für Konsolen-Eingaben (Maus, Tasten, Fenstergröße).
// Begriffe wie DllImport, StructLayout, FieldOffset und nint sind fortgeschritten.
// Wir lassen das so, weil es für die Windows-Konsole technisch nötig ist.
// Du musst das jetzt noch nicht im Detail beherrschen.
internal static class ConsoleInputWindows
{
    internal const int StdInputHandle = -10;

    internal const ushort KeyEvent = 0x0001;
    internal const ushort MouseEvent = 0x0002;
    internal const ushort WindowBufferSizeEvent = 0x0004;

    internal const uint FromLeft1stButtonPressed = 0x0001;
    internal const uint MouseMoved = 0x0001;

    // Liest ein Handle auf die Standard-Eingabe der Konsole.
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern nint GetStdHandle(int nStdHandle);

    // Fragt ab, wie viele Eingabe-Events anstehen.
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool GetNumberOfConsoleInputEvents(nint hConsoleInput, out uint lpcNumberOfEvents);

    // Liest Events (Taste/Maus/Fensteränderung) aus der Windows-Konsole.
    [DllImport("kernel32.dll", EntryPoint = "ReadConsoleInputW", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern bool ReadConsoleInput(
        nint hConsoleInput,
        [Out] InputRecord[] lpBuffer,
        uint nLength,
        out uint lpNumberOfEventsRead);

    // Liest den aktuellen Konsolenmodus.
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);

    // Setzt den Konsolenmodus.
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool SetConsoleMode(nint hConsoleHandle, uint dwMode);

    // Wartet auf Eingabe für maximal X Millisekunden.
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern uint WaitForSingleObject(nint hHandle, uint dwMilliseconds);

    internal const uint EnableExtendedFlags = 0x0080;
    internal const uint EnableQuickEditMode = 0x0040;
    internal const uint EnableMouseInput = 0x0010;
    internal const uint EnableWindowInput = 0x0008;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Coord
    {
        internal short X;
        internal short Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KeyEventRecord
    {
        internal int KeyDown;
        internal ushort RepeatCount;
        internal ushort VirtualKeyCode;
        internal ushort VirtualScanCode;
        internal char UnicodeChar;
        internal uint ControlKeyState;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MouseEventRecord
    {
        internal Coord MousePosition;
        internal uint ButtonState;
        internal uint ControlKeyState;
        internal uint EventFlags;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct InputRecord
    {
        [FieldOffset(0)]
        internal ushort EventType;
        [FieldOffset(4)]
        internal KeyEventRecord KeyEvent;
        [FieldOffset(4)]
        internal MouseEventRecord MouseEvent;
    }

    // Aktiviert Maus- und Fenster-Events in der Konsole.
    internal static void EnableMouseAndWindowInput()
    {
        nint hIn = GetStdHandle(StdInputHandle);
        if (hIn == nint.Zero || hIn == new nint(-1))
            return;

        if (!GetConsoleMode(hIn, out uint mode))
            return;

        mode |= EnableExtendedFlags;
        mode &= ~EnableQuickEditMode;
        mode |= EnableMouseInput;
        mode |= EnableWindowInput;
        SetConsoleMode(hIn, mode);
    }

    // Versucht genau EIN Event zu lesen.
    internal static bool TryReadOneInput(nint hIn, out InputRecord record, out bool hasRecord)
    {
        record = default;
        hasRecord = false;
        if (hIn == nint.Zero || hIn == new nint(-1))
            return false;

        if (!GetNumberOfConsoleInputEvents(hIn, out uint n) || n == 0)
            return true;

        var buf = new InputRecord[1];
        if (!ReadConsoleInput(hIn, buf, 1, out uint read) || read == 0)
            return false;

        record = buf[0];
        hasRecord = true;
        return true;
    }

    // Wartet kurz auf neue Eingaben; Fallback über Sleep ohne gültiges Handle.
    internal static void WaitForInput(nint hIn, int milliseconds)
    {
        if (hIn == nint.Zero || hIn == new nint(-1))
        {
            Thread.Sleep(milliseconds);
            return;
        }

        WaitForSingleObject(hIn, (uint)milliseconds);
    }
}

// Was macht diese Datei?
// - Kapselt native Windows-Konsolenfunktionen (Tastatur, Maus, Fenster-Events).
// - Liefert einfache Hilfsmethoden, die der restliche Admin-Code aufruft.
