using System.Text;
using System.Diagnostics;

namespace StartScreen
{
    public class StartScreenView
    {
        // Bisschen Start-Show und dann normal weiter.
        private const string BrandName = "syscore";
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

        public void ShowStartScreen()
        {
            if (Console.IsOutputRedirected)
            {
                Console.Out.WriteLine(BrandName);
                Thread.Sleep(TimeSpan.FromSeconds(2.4));
                TryRunGitUpdateCheck();
                return;
            }

            bool isWin = OperatingSystem.IsWindows();
            bool oldCursorVisible = isWin && Console.CursorVisible;
            Encoding oldEncoding = Console.OutputEncoding;
            using var restore = new ConsoleRestoreScope(oldEncoding, isWin, oldCursorVisible);

            Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            if (isWin)
                Console.CursorVisible = false;

            int w = Console.WindowWidth;
            int h = Console.WindowHeight;
            if (w <= 0) w = 80;
            if (h <= 0) h = 25;

            Console.Clear();
            Console.ResetColor();
            string[] banner = PickBanner(w);
            int startRow = Math.Max(0, (h - banner.Length - 5) / 2);
            WriteBannerCentered(banner, startRow, w);

            int nameRow = startRow + banner.Length;
            if (ReferenceEquals(banner, BannerSyscoreWide))
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                WriteCentered(BrandName, nameRow, w);
                Console.ResetColor();
                nameRow++;
            }

            int loadRow = Math.Min(h - 1, nameRow + 1);
            RunLoadingLine(loadRow, w, TimeSpan.FromSeconds(2.4));
            TryRunGitUpdateCheck();
        }

        private static void TryRunGitUpdateCheck()
        {
            try
            {
            var fetch = RunGit("fetch");
                if (!fetch.ok)
                {
                    Console.WriteLine("Git fetch ging nicht. (Vielleicht kein Netz oder git?)");
                    return;
                }

                var behind = RunGit("rev-list --count HEAD..origin/main");
                if (!behind.ok) return;
                int cnt = 0;
                int.TryParse((behind.output ?? "").Trim(), out cnt);
                if (cnt <= 0) return;

                Console.WriteLine("Neue Version da. Commits anschauen und pullen? [J/N]");
                string? yn = Console.ReadLine();
                if (!(yn ?? "").Trim().Equals("j", StringComparison.OrdinalIgnoreCase))
                    return;

                var log = RunGit("log HEAD..origin/main --format=\"- %s%n  %b\"");
                if (log.ok)
                {
                    ShowCommitBlocks(log.output ?? "");
                }
                Console.WriteLine($"Es gibt {cnt} neue Commits.");
                Console.WriteLine("Enter drücken zum Installieren...");
                Console.ReadLine();

                var pull = RunGit("pull");
                if (!pull.ok)
                {
                    Console.WriteLine("❌ Update fehlgeschlagen: " + pull.output);
                    return;
                }
                Console.WriteLine("Update fertig. Starte Programm neu...");
                RestartCurrentProcess();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Update-Check Fehler: " + ex.Message);
            }
        }

        private static void ShowCommitBlocks(string all)
        {
            string[] lines = all.Replace("\r", "").Split('\n');
            StringBuilder tmp = new();
            foreach (var line in lines)
            {
                if (line.StartsWith("- "))
                {
                    if (tmp.Length > 0)
                    {
                        Console.WriteLine("════════════════════════════════════");
                        Console.Write(tmp.ToString());
                        Console.WriteLine("════════════════════════════════════");
                        tmp.Clear();
                    }
                    tmp.AppendLine("📦 Commit: " + line.Substring(2));
                }
                else
                {
                    tmp.AppendLine("   " + line);
                }
            }
            if (tmp.Length > 0)
            {
                Console.WriteLine("════════════════════════════════════");
                Console.Write(tmp.ToString());
                Console.WriteLine("════════════════════════════════════");
            }
        }

        private static (bool ok, string output) RunGit(string args)
        {
            try
            {
                ProcessStartInfo psi = new("git", args);
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                using Process p = Process.Start(psi)!;
                string o = p.StandardOutput.ReadToEnd();
                string e = p.StandardError.ReadToEnd();
                p.WaitForExit();
                bool ok = p.ExitCode == 0;
                return (ok, ok ? o : (o + Environment.NewLine + e));
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private static void RestartCurrentProcess()
        {
            string? exe = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(exe))
                return;

            string[] args = Environment.GetCommandLineArgs();
            string joined = "";
            for (int i = 1; i < args.Length; i++)
            {
                if (i > 1) joined += " ";
                joined += "\"" + args[i].Replace("\"", "\\\"") + "\"";
            }

            ProcessStartInfo psi = new();
            psi.FileName = exe;
            psi.Arguments = joined;
            psi.UseShellExecute = true;
            Process.Start(psi);
            Environment.Exit(0);
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

        private static string[] PickBanner(int windowWidth)
        {
            int inner = Math.Max(0, windowWidth - 2);
            if (inner >= MaxDisplayWidth(BannerSyscoreWide))
                return BannerSyscoreWide;
            return BuildFramedTitle(windowWidth, BrandName);
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

        private static void RunLoadingLine(int row, int windowWidth, TimeSpan duration)
        {
            if (row < 0 || row >= Console.WindowHeight) return;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            const int barLen = 10;

            while (sw.Elapsed < duration)
            {
                double p = sw.Elapsed.TotalMilliseconds / duration.TotalMilliseconds;
                if (p < 0) p = 0;
                if (p > 1) p = 1;
                int full = (int)Math.Round(p * barLen);
                if (full < 0) full = 0;
                if (full > barLen) full = barLen;
                string b = new string('█', full) + new string('░', barLen - full);
                string msg = $"Laden… {b}";
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
