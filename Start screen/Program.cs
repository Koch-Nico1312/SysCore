using System.Text;

namespace StartScreen
{
    public class startScreen
    {
        /// <summary>Brand name shown in the narrow (framed) layout and under the wide banner.</summary>
        private const string BrandName = "syscore";

        /// <summary>
        /// Wide FIGlet-style ASCII art spelling <c>SYSCORE</c> (letters S-Y-S-C-O-R-E).
        /// Shown only when the console is wide enough (~52+ columns for this glyph set).
        /// </summary>
        private static readonly string[] BannerSyscoreWide =
        [
            @"  /$$$$$$                       /$$$$$$                               ",
            @" /$$__  $$                     /$$__  $$                              ",
            @"| $$  \__/ /$$   /$$  /$$$$$$$| $$  \__/  /$$$$$$   /$$$$$$   /$$$$$$ ",
            @"|  $$$$$$ | $$  | $$ /$$_____/| $$       /$$__  $$ /$$__  $$ /$$__  $$",
            @" \____  $$| $$  | $$|  $$$$$$ | $$      | $$  \ $$| $$  \__/| $$$$$$$$",
            @" /$$  \ $$| $$  | $$ \____  $$| $$    $$| $$  | $$| $$      | $$_____/",
            @"|  $$$$$$/|  $$$$$$$ /$$$$$$$/|  $$$$$$/|  $$$$$$/| $$      |  $$$$$$$",
            @" \______/  \____  $$|_______/  \______/  \______/ |__/       \_______/",
            @"           /$$  | $$                                                  ",
            @"          |  $$$$$$/                                                  ",
            @"           \______/                                                  "
        ];

        /// <summary>
        /// Clears the console, draws a centered banner (size depends on <see cref="Console.WindowWidth"/>),
        /// then runs a short loading animation. Restores cursor and encoding when finished.
        /// </summary>
        public void ShowStartScreen()
        {
            if (Console.IsOutputRedirected)
            {
                Console.Out.WriteLine(BrandName);
                Thread.Sleep(TimeSpan.FromSeconds(2.4));
                return;
            }

            bool hadCursorState = OperatingSystem.IsWindows();
            bool savedVisible = hadCursorState && Console.CursorVisible;
            var savedEncoding = Console.OutputEncoding;
            using var restore = new ConsoleRestoreScope(savedEncoding, hadCursorState, savedVisible);

            Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            if (hadCursorState)
                Console.CursorVisible = false;

            int w = Console.WindowWidth;
            int h = Console.WindowHeight;
            if (w <= 0) w = 80;
            if (h <= 0) h = 25;

            Console.Clear();
            Console.ResetColor();

            // Pick either the large ASCII wordmark or a full-width framed title.
            string[] banner = PickBanner(w);

            // Vertical centering: leave a few rows below for the optional subtitle + loading line.
            int startRow = Math.Max(0, (h - banner.Length - 5) / 2);
            WriteBannerCentered(banner, startRow, w);

            int nameRow = startRow + banner.Length;
            // Under the wide FIGlet, repeat the brand in normal text (lowercase as requested).
            if (ReferenceEquals(banner, BannerSyscoreWide))
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                WriteCentered(BrandName, nameRow, w);
                Console.ResetColor();
                nameRow++;
            }

            int loadRow = Math.Min(h - 1, nameRow + 1);
            RunLoadingLine(loadRow, w, TimeSpan.FromSeconds(2.4));
        }

        private sealed class ConsoleRestoreScope : IDisposable
        {
            private readonly Encoding _encoding;
            private readonly bool _restoreCursor;
            private readonly bool _cursorVisible;

            public ConsoleRestoreScope(Encoding encoding, bool restoreCursor, bool cursorVisible)
            {
                _encoding = encoding;
                _restoreCursor = restoreCursor;
                _cursorVisible = cursorVisible;
            }

            public void Dispose()
            {
                if (_restoreCursor)
                    Console.CursorVisible = _cursorVisible;
                Console.OutputEncoding = _encoding;
            }
        }

        /// <summary>
        /// Returns the wide ASCII banner if it fits inside the window; otherwise a box that scales to <paramref name="windowWidth"/>.
        /// </summary>
        private static string[] PickBanner(int windowWidth)
        {
            int inner = Math.Max(0, windowWidth - 2);
            if (inner >= MaxDisplayWidth(BannerSyscoreWide))
                return BannerSyscoreWide;
            return BuildFramedTitle(windowWidth, BrandName);
        }

        /// <summary>Longest visual line width in the banner (used to decide if the wide art fits).</summary>
        private static int MaxDisplayWidth(IReadOnlyList<string> lines)
        {
            int m = 0;
            foreach (var line in lines)
            {
                int len = GetDisplayWidth(line);
                if (len > m) m = len;
            }
            return m;
        }

        /// <summary>
        /// Approximate terminal column width of <paramref name="s"/> (wide East Asian characters count as 2).
        /// ASCII FIGlet lines are all width 1 per code point.
        /// </summary>
        private static int GetDisplayWidth(string s)
        {
            int w = 0;
            foreach (var r in s.EnumerateRunes())
            {
                w += r.Value is >= 0x1100 and <= 0x115F
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
            return w;
        }

        /// <summary>Draws each banner line in cyan, horizontally centered within <paramref name="windowWidth"/>.</summary>
        private static void WriteBannerCentered(IReadOnlyList<string> lines, int startRow, int windowWidth)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                int dw = GetDisplayWidth(line);
                int pad = Math.Max(0, (windowWidth - dw) / 2);
                int row = startRow + i;
                if (row >= 0 && row < Console.WindowHeight)
                {
                    Console.SetCursorPosition(0, row);
                    Console.Write(new string(' ', windowWidth));
                    Console.SetCursorPosition(pad, row);
                    Console.Write(line);
                }
            }
            Console.ResetColor();
        }

        /// <summary>Writes a single line of text centered on row <paramref name="row"/>.</summary>
        private static void WriteCentered(string text, int row, int windowWidth)
        {
            if (row < 0 || row >= Console.WindowHeight) return;
            int dw = GetDisplayWidth(text);
            int pad = Math.Max(0, (windowWidth - dw) / 2);
            Console.SetCursorPosition(0, row);
            Console.Write(new string(' ', windowWidth));
            Console.SetCursorPosition(pad, row);
            Console.Write(text);
        }

        /// <summary>Builds a three-line box; inner width tracks the console so narrow terminals still get a full-width frame.</summary>
        private static string[] BuildFramedTitle(int windowWidth, string title)
        {
            int inner = Math.Max(4, windowWidth - 4);
            string top = "╔" + new string('═', inner) + "╗";
            string bottom = "╚" + new string('═', inner) + "╝";

            int titleW = GetDisplayWidth(title);
            int pad = Math.Max(0, inner - titleW);
            int left = pad / 2;
            int right = pad - left;
            string mid = "║" + new string(' ', left) + title + new string(' ', right) + "║";

            return [top, mid, bottom];
        }

        /// <summary>Centered spinner on one row for <paramref name="duration"/>, then clears that row.</summary>
        private static void RunLoadingLine(int row, int windowWidth, TimeSpan duration)
        {
            if (row < 0 || row >= Console.WindowHeight) return;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            ReadOnlySpan<string> spin = ["|", "/", "-", "\\"];

            while (sw.Elapsed < duration)
            {
                string phase = spin[(int)(sw.Elapsed.TotalMilliseconds / 120 % spin.Length)];
                string msg = $"Laden… {phase}";
                int dw = GetDisplayWidth(msg);
                int pad = Math.Max(0, (windowWidth - dw) / 2);

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.SetCursorPosition(0, row);
                Console.Write(new string(' ', windowWidth));
                Console.SetCursorPosition(pad, row);
                Console.Write(msg);
                Console.ResetColor();

                Thread.Sleep(50);
            }

            Console.SetCursorPosition(0, row);
            Console.Write(new string(' ', windowWidth));
        }
    }
}
