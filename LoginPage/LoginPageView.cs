namespace LoginPage
{
    public class LoginPageView
    {
        private static readonly string[] adminNames = new string[]
        {
            "Nico",
            "Palitsch",
            "PALI"
        };

        // Zeigt den Login und liefert true bei Admin-Namen.
        public bool ShowLogin()
        {
            Console.Clear();
            Console.ResetColor();

            (int width, int height) = GetConsoleSize();
            int top = Math.Max(0, (height - 6) / 2);

            WriteCenteredAt("═══ 𝓛𝓸𝓰𝓲𝓷 ═══", top, ConsoleColor.Cyan, width);

            string prompt = "Wie heisst du? ";
            int promptLeft = Math.Max(0, (width - prompt.Length) / 2);
            int promptTop = top + 2;
            Console.SetCursorPosition(promptLeft, promptTop);
            Console.Write(prompt);
            string name = (Console.ReadLine() ?? string.Empty).Trim();

            bool isAdmin = IsAdminName(name);
            WriteCenteredAt("Eingabe: " + (name.Length == 0 ? "(leer)" : name), promptTop + 2, null, width);

            if (isAdmin)
            {
                WriteCenteredAt("Admin erkannt - Zugriff gewaehrt.", promptTop + 3, ConsoleColor.Green, width);
            }
            else
            {
                WriteCenteredAt("Kein Admin - normaler Zugriff.", promptTop + 3, ConsoleColor.Yellow, width);
            }

            Console.ResetColor();
            return isAdmin;
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

        // Prueft, ob ein Name in der Admin-Liste steht.
        private static bool IsAdminName(string name)
        {
            if (name.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < adminNames.Length; i++)
            {
                if (string.Equals(adminNames[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
