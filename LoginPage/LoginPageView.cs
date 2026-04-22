namespace LoginPage
{
    public class LoginPageView
    {
        private const int maxTry = 3;

        private static readonly string[] loginHeadLine =
        [
            @"      :::        ::::::::   :::::::: ::::::::::: ::::    ::: ",
            @"     :+:       :+:    :+: :+:    :+:    :+:     :+:+:   :+: ",
            @"    +:+       +:+    +:+ +:+           +:+     :+:+:+  +:+   ",
            @"   +#+       +#+    +:+ :#:           +#+     +#+ +:+ +#+    ",
            @"  +#+       +#+    +#+ +#+   +#+#    +#+     +#+  +#+#+#     ",
            @" #+#       #+#    #+# #+#    #+#    #+#     #+#   #+#+#      ",
            @"########## ########   ######## ########### ###    ####       ",
        ];

        private static readonly string[] adminNames =
        [
            "nico",
            "palitsch",
            "pali"
        ];

        // Zeigt den Login und liefert true bei Admin-Namen.
        public bool ShowLogin()
        {
            Console.Clear();
            Console.ResetColor();

            (int width, int height) = GetConsoleSize();

            // Banner vertikal mittig – Bannerhöhe (7) + 2 Abstandszeilen berücksichtigen
            int bannerHeight = loginHeadLine.Length;
            int bannerTop = Math.Max(0, (height - bannerHeight - 6) / 2);
            int infoTop  = bannerTop + bannerHeight + 1;
            int promptTop = infoTop + 2;

            WriteBannerCentered(loginHeadLine, bannerTop, ConsoleColor.Cyan, width);
            WriteCenteredAt("Bitte Namen eingeben (oder 'exit' zum Beenden).", infoTop, ConsoleColor.Gray, width);

            for (int attempt = 1; attempt <= maxTry; attempt++)
            {
                string prompt = $"Wie heißt du?  [{attempt}/{maxTry}] ";
                int promptLeft = Math.Max(0, (width - prompt.Length) / 2);
                Console.SetCursorPosition(promptLeft, promptTop);
                Console.Write(new string(' ', Math.Max(0, width - promptLeft)));
                Console.SetCursorPosition(promptLeft, promptTop);
                Console.Write(prompt);

                string name = NormalizeName(Console.ReadLine()!.ToLower());
                if (name.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    WriteCenteredAt("Login abgebrochen.", promptTop + 2, ConsoleColor.Yellow, width);
                    Console.ResetColor();
                    break;
                }

                if (name.Length == 0)
                {
                    WriteCenteredAt("Leere Eingabe ist nicht erlaubt.", promptTop + 2, ConsoleColor.Red, width);
                    continue;
                }

                bool isAdmin = IsAdminName(name);
                WriteCenteredAt("Eingabe: " + name, promptTop + 2, ConsoleColor.Gray, width);

                if (isAdmin)
                {
                    WriteCenteredAt("Admin erkannt - Zugriff gewährt.", promptTop + 3, ConsoleColor.Green, width);
                    Console.ResetColor();
                    return true;
                }

                if (attempt < maxTry)
                    WriteCenteredAt("Kein Admin. Bitte erneut versuchen.", promptTop + 3, ConsoleColor.Yellow, width);
            }

            WriteCenteredAt("Kein Admin erkannt - normaler Zugriff.", promptTop + 3, ConsoleColor.Yellow, width);
            Console.ResetColor();
            return false;
        }

        private static (int Width, int Height) GetConsoleSize()
        {
            int width  = Console.WindowWidth  > 0 ? Console.WindowWidth  : 80;
            int height = Console.WindowHeight > 0 ? Console.WindowHeight : 25;
            return (width, height);
        }

        // Mehrzeiliges Banner korrekt zentrieren – alle Zeilen gleich weit einrücken.
        private static void WriteBannerCentered(string[] lines, int startRow, ConsoleColor color, int width)
        {
            // Längste Zeile bestimmen → einheitlicher Einzug für alle Zeilen
            int maxLen = 0;
            foreach (var l in lines)
                if (l.Length > maxLen) maxLen = l.Length;

            int pad = Math.Max(0, (width - maxLen) / 2);

            Console.ForegroundColor = color;
            for (int i = 0; i < lines.Length; i++)
            {
                int row = Math.Clamp(startRow + i, 0, Math.Max(0, Console.BufferHeight - 1));
                Console.SetCursorPosition(pad, row);
                Console.Write(lines[i]);
            }
            Console.ResetColor();
        }

        // Einzelne Textzeile horizontal zentrieren.
        private static void WriteCenteredAt(string text, int top, ConsoleColor? color, int width)
        {
            int safeTop = Math.Clamp(top, 0, Math.Max(0, Console.BufferHeight - 1));
            int left    = Math.Max(0, (width - text.Length) / 2);
            Console.SetCursorPosition(left, safeTop);
            if (color.HasValue) Console.ForegroundColor = color.Value;
            Console.Write(text);
            Console.ResetColor();
        }

        // Prüft, ob ein Name in der Admin-Liste steht.
        private static bool IsAdminName(string name)
        {
            if (name.Length == 0) return false;
            for (int i = 0; i < adminNames.Length; i++)
                if (string.Equals(adminNames[i], name, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        // Leere Eingabe abfangen damit das Programm nicht abstürzt.
        private static string NormalizeName(string? input)
            => (input ?? string.Empty).Trim();
    }
}
