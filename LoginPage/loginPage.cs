using System.Text;

namespace LoginPage
{
    /// <summary>
    /// Login screen after the start screen: asks for a name (ASCII title), echoes the input in color, checks admin names case-insensitively.
    /// </summary>
    public class loginPage
    {
        private static readonly HashSet<string> AdminNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Nico",
            "Palitsch",
            "PALI"
        };

        private static readonly string[] BannerLoginWide =
        [
            @" __        __   _                            ",
            @" \ \      / /__| | ___ ___  _ __ ___   ___   ",
            @"  \ \ /\ / / _ \ |/ __/ _ \| '_ ` _ \ / _ \  ",
            @"   \ V  V /  __/ | (_| (_) | | | | | |  __/  ",
            @"    \_/\_/ \___|_|\___\___/|_| |_| |_|\___|  "
        ];

        /// <summary>Shows the login UI and returns whether the entered name is an admin.</summary>
        public bool ShowLogin()
        {
            if (Console.IsOutputRedirected)
                return RunLoginPlain();

            var savedEncoding = Console.OutputEncoding;
            using var restore = new ConsoleEncodingRestore(savedEncoding);
            Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            return RunLoginVisual();
        }

        private static bool RunLoginPlain()
        {
            Console.Out.WriteLine("LOGIN");
            Console.Out.Write("Wie heisst du? ");
            string? raw = Console.ReadLine();
            string name = raw?.Trim() ?? "";
            bool isAdmin = !string.IsNullOrEmpty(name) && AdminNames.Contains(name);
            Console.Out.WriteLine($"Eingabe: {name}");
            Console.Out.WriteLine(isAdmin ? "Admin erkannt - Zugriff gewaehrt." : "Kein Admin - normaler Zugriff.");
            return isAdmin;
        }

        private static bool RunLoginVisual()
        {
            Console.CursorVisible = true;

            Console.Clear();
            Console.ResetColor();

            int w = Console.WindowWidth;
            int h = Console.WindowHeight;

            if (w <= 0) w = 80;
            if (h <= 0) h = 25;

            string[] banner = PickBanner(w);
            int startRow = Math.Max(0, (h - banner.Length - 8) / 2);
            WriteBannerCentered(banner, startRow, w);

            int promptRow = Math.Min(h - 1, startRow + banner.Length + 2);
            int pad = Math.Max(0, (w - 40) / 2);

            Console.SetCursorPosition(0, promptRow);
            Console.Write(new string(' ', w));
            Console.SetCursorPosition(pad, promptRow);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Wie heißt du? ");
            Console.ResetColor();

            string? raw = Console.ReadLine();
            string name = raw?.Trim() ?? "";

            int echoRow = Math.Min(h - 1, promptRow + 2);
            Console.SetCursorPosition(0, echoRow);
            Console.Write(new string(' ', w));
            Console.SetCursorPosition(pad, echoRow);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Eingabe: ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(string.IsNullOrEmpty(name) ? "(leer)" : name);
            Console.ResetColor();

            int resultRow = Math.Min(h - 1, echoRow + 2);
            Console.SetCursorPosition(0, resultRow);
            Console.Write(new string(' ', w));
            Console.SetCursorPosition(pad, resultRow);

            bool isAdmin = !string.IsNullOrEmpty(name) && AdminNames.Contains(name);
            if (isAdmin)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Admin erkannt – Zugriff gewährt.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Kein Admin – normaler Zugriff.");
            }

            Console.ResetColor();
            Console.SetCursorPosition(0, Math.Min(h - 1, resultRow + 2));

            return isAdmin;
        }

        private static string[] PickBanner(int windowWidth)
        {
            int inner = Math.Max(0, windowWidth - 2);
            if (inner >= MaxDisplayWidth(BannerLoginWide))
                return BannerLoginWide;
            return BuildFramedTitle(windowWidth, "login");
        }

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

        private static int GetDisplayWidth(string s)
        {
            int width = 0;
            foreach (var r in s.EnumerateRunes())
            {
                width += r.Value is >= 0x1100 and <= 0x115F
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
            return width;
        }

        private static void WriteBannerCentered(IReadOnlyList<string> lines, int startRow, int windowWidth)
        {
            ConsoleColor[] stripe = [ConsoleColor.Magenta, ConsoleColor.Cyan, ConsoleColor.Yellow, ConsoleColor.Green, ConsoleColor.Red];
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                int dw = GetDisplayWidth(line);
                int pad = Math.Max(0, (windowWidth - dw) / 2);
                int row = startRow + i;
                if (row >= 0 && row < Console.WindowHeight)
                {
                    Console.ForegroundColor = stripe[i % stripe.Length];
                    Console.SetCursorPosition(0, row);
                    Console.Write(new string(' ', windowWidth));
                    Console.SetCursorPosition(pad, row);
                    Console.Write(line);
                }
            }

            Console.ResetColor();
        }

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

        private sealed class ConsoleEncodingRestore : IDisposable
        {
            private readonly Encoding _encoding;

            public ConsoleEncodingRestore(Encoding encoding) => _encoding = encoding;

            public void Dispose() => Console.OutputEncoding = _encoding;
        }
    }
}
