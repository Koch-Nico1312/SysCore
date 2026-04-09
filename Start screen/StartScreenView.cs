namespace StartScreen
{
    public class StartScreenView
    {
        private static readonly string[] banner = new string[]
        {
            "  _____         _____                     ",
            " / ____|       / ____|                    ",
            "| (___  _   _ | (___   ___ ___  _ __ ___  ",
            " \\___ \\| | | | \\___ \\ / __/ _ \\| '__/ _ \\ ",
            " ____) | |_| | ____) | (_| (_) | | |  __/ ",
            "|_____/ \\__, ||_____/ \\___\\___/|_|  \\___| ",
            "         __/ |                             ",
            "        |___/                              "
        };

        // Zeigt den Startbildschirm mit kurzem Ladeeffekt.
        public void ShowStartScreen()
        {
            Console.Clear();
            Console.ResetColor();

            (int width, int height) = GetConsoleSize();
            int contentHeight = banner.Length + 4;
            int top = Math.Max(0, (height - contentHeight) / 2);

            WriteCenteredAt("⏳ 𝓛𝓪𝓭𝓮𝓼𝓬𝓻𝓮𝓮𝓷 ⏳", top, ConsoleColor.Yellow, width);
            PrintBanner(top + 2, width);
            RunSimpleLoading(top + 2 + banner.Length + 1, width);
        }

        // Zeichnet das ASCII-Banner zentriert in der Konsole.
        private static void PrintBanner(int top, int width)
        {
            for (int i = 0; i < banner.Length; i++)
            {
                WriteCenteredAt(banner[i], top + i, ConsoleColor.Cyan, width);
            }
        }

        // Zeigt einen einfachen Ladebalken fuer den Start.
        private static void RunSimpleLoading(int top, int width)
        {
            string prefix = "Laden: [";
            string suffix = "]";
            const int barLength = 20;
            int left = Math.Max(0, (width - (prefix.Length + barLength + suffix.Length)) / 2);

            int safeTop = Math.Clamp(top, 0, Math.Max(0, Console.BufferHeight - 1));
            Console.SetCursorPosition(left, safeTop);
            Console.Write(prefix);

            for (int i = 0; i < 20; i++)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("#");
                Console.ResetColor();
                Thread.Sleep(80);
            }

            Console.WriteLine(suffix);
            Thread.Sleep(300);
        }

        private static (int Width, int Height) GetConsoleSize()
        {
            int width = Console.WindowWidth > 0 ? Console.WindowWidth : 80;
            int height = Console.WindowHeight > 0 ? Console.WindowHeight : 25;
            return (width, height);
        }

        private static void WriteCenteredAt(string text, int top, ConsoleColor? color, int width)
        {
            int safeTop = Math.Clamp(top, 0, Math.Max(0, Console.BufferHeight - 1));
            int left = Math.Max(0, (width - text.Length) / 2);
            Console.SetCursorPosition(left, safeTop);

            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }

            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
