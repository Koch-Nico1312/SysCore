using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AdminApp;

public sealed partial class AdminPortal
{
    // ===================== Produktivität =====================

    private void RunPomodoroTimer()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ ⏱️  Pomodoro-Timer           ║");
        Console.WriteLine("╚══════════════════════════════╝");
        Console.Write("Minuten (Standard 25): ");
        string? inp = Console.ReadLine();
        if (!int.TryParse(inp, out int min) || min < 1) min = 25;
        int sek = min * 60;
        Console.WriteLine("Start — beliebige Taste zum Abbrechen.");
        DateTime end = DateTime.Now.AddSeconds(sek);
        while (DateTime.Now < end)
        {
            if (Console.KeyAvailable)
            {
                Console.ReadKey(intercept: true);
                Console.WriteLine("\nAbgebrochen.");
                return;
            }
            int rest = (int)(end - DateTime.Now).TotalSeconds;
            Console.Write($"\r⏳ {rest / 60:D2}:{rest % 60:D2} ");
            Thread.Sleep(1000);
        }
        Console.WriteLine("\n✅ Zeit abgelaufen!");
    }

    private sealed record ContactEntry(string Name, string Phone, string Email, string Note);

    private void RunContactBook()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ 📇 Kontaktbuch               ║");
        Console.WriteLine("╚══════════════════════════════╝");
        bool end = false;
        while (!end)
        {
            Console.WriteLine("1) Liste  2) Neu  3) Suchen  4) Löschen  5) Ende");
            Console.Write("> ");
            string? k = Console.ReadLine();
            switch (k?.Trim() ?? "")
            {
                case "5": end = true; break;
                case "1": ShowContactList(); break;
                case "2": AddContact(); break;
                case "3": SearchContact(); break;
                case "4": DeleteContact(); break;
            }
        }
    }

    private static string BuildContactsFilePath()
    {
        return Path.Combine(GetSysCoreDataDirectory(), "admin_kontakte.txt");
    }

    private List<ContactEntry> LoadContacts()
    {
        var list = new List<ContactEntry>();
        string path = BuildContactsFilePath();
        if (!File.Exists(path)) return list;
        foreach (var raw in File.ReadAllLines(path, Encoding.UTF8))
        {
            var p = raw.Split('|');
            if (p.Length < 4) continue;
            list.Add(new ContactEntry(p[0], p[1], p[2], p[3]));
        }
        return list;
    }

    private void SaveContacts(List<ContactEntry> list)
    {
        var lines = list.Select(c => $"{c.Name}|{c.Phone}|{c.Email}|{c.Note}").ToArray();
        File.WriteAllLines(BuildContactsFilePath(), lines, Encoding.UTF8);
    }

    private void ShowContactList()
    {
        var list = LoadContacts();
        if (list.Count == 0) { Console.WriteLine("(leer)"); return; }
        for (int i = 0; i < list.Count; i++)
            Console.WriteLine($"{i + 1}. {list[i].Name} | {list[i].Phone} | {list[i].Email}");
    }

    private void AddContact()
    {
        Console.Write("Name: "); string name = Console.ReadLine()?.Trim() ?? "";
        if (name.Length == 0) return;
        Console.Write("Telefon: "); string phone = Console.ReadLine()?.Trim() ?? "";
        Console.Write("E-Mail: "); string email = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Notiz: "); string note = Console.ReadLine()?.Trim() ?? "";
        var list = LoadContacts();
        list.Add(new ContactEntry(name, phone, email, note));
        SaveContacts(list);
        Console.WriteLine("✅ Gespeichert.");
    }

    private void SearchContact()
    {
        Console.Write("Suchbegriff: "); string q = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";
        if (q.Length == 0) return;
        var list = LoadContacts();
        int found = 0;
        foreach (var c in list)
        {
            if (c.Name.ToLowerInvariant().Contains(q) || c.Email.ToLowerInvariant().Contains(q))
            {
                Console.WriteLine($"  {c.Name} | {c.Phone} | {c.Email}");
                found++;
            }
        }
        Console.WriteLine(found == 0 ? "Nichts gefunden." : $"{found} Treffer.");
    }

    private void DeleteContact()
    {
        var list = LoadContacts();
        ShowContactList();
        Console.Write("Nummer zum Löschen: ");
        if (!int.TryParse(Console.ReadLine(), out int idx) || idx < 1 || idx > list.Count) return;
        list.RemoveAt(idx - 1);
        SaveContacts(list);
        Console.WriteLine("✅ Gelöscht.");
    }

    // ===================== Tools & Rechner =====================

    private void RunHashGenerator()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ #️⃣ Hash-Generator            ║");
        Console.WriteLine("╚══════════════════════════════╝");
        Console.WriteLine("1) MD5   2) SHA-1   3) SHA-256   4) SHA-512   0) Ende");
        Console.Write("> ");
        string? k = Console.ReadLine()?.Trim();
        if (k == "0") return;
        Console.Write("Text: ");
        string text = Console.ReadLine() ?? "";
        byte[] data = Encoding.UTF8.GetBytes(text);
        string hash = k switch
        {
            "1" => Convert.ToHexString(MD5.HashData(data)),
            "2" => Convert.ToHexString(SHA1.HashData(data)),
            "3" => Convert.ToHexString(SHA256.HashData(data)),
            "4" => Convert.ToHexString(SHA512.HashData(data)),
            _ => "Ungültig"
        };
        Console.WriteLine($"Hash: {hash.ToLowerInvariant()}");
    }

    private void RunBase64Tool()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ 🔤 Base64 En/Decoder         ║");
        Console.WriteLine("╚══════════════════════════════╝");
        Console.WriteLine("1) Encode Text   2) Decode Text   3) Encode Datei   0) Ende");
        Console.Write("> ");
        string? k = Console.ReadLine()?.Trim();
        if (k == "0") return;
        if (k == "1")
        {
            Console.Write("Text: "); string t = Console.ReadLine() ?? "";
            Console.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(t)));
        }
        else if (k == "2")
        {
            Console.Write("Base64: "); string b = Console.ReadLine()?.Trim() ?? "";
            try { Console.WriteLine(Encoding.UTF8.GetString(Convert.FromBase64String(b))); }
            catch { Console.WriteLine("❌ Ungültiger Base64-String."); }
        }
        else if (k == "3")
        {
            Console.Write("Dateipfad: "); string p = Console.ReadLine()?.Trim() ?? "";
            if (!File.Exists(p)) { Console.WriteLine("❌ Datei nicht gefunden."); return; }
            Console.WriteLine(Convert.ToBase64String(File.ReadAllBytes(p)));
        }
    }

    private void RunPasswordStrengthChecker()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ 🔒 Passwort-Stärke-Checker   ║");
        Console.WriteLine("╚══════════════════════════════╝");
        Console.Write("Passwort: ");
        string pw = Console.ReadLine() ?? "";
        int score = 0;
        if (pw.Length >= 8) score++;
        if (pw.Length >= 12) score++;
        if (pw.Any(char.IsUpper)) score++;
        if (pw.Any(char.IsLower)) score++;
        if (pw.Any(char.IsDigit)) score++;
        if (pw.Any(c => !char.IsLetterOrDigit(c))) score++;
        double entropy = pw.Length * Math.Log2(pw.Distinct().Count() > 1 ? pw.Distinct().Count() : 2);
        string label = score switch { <= 2 => "🟠 Schwach", <= 4 => "🟡 Mittel", _ => "🟢 Stark" };
        Console.WriteLine($"{label} (Score {score}/6, Entropie ~{entropy:0} Bits)");
    }

    // ===================== System & Netzwerk =====================

    private void RunProcessLister()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ 📈 Prozess-Lister            ║");
        Console.WriteLine("╚══════════════════════════════╝");
        var procs = Process.GetProcesses().OrderBy(p => p.ProcessName).ToList();
        for (int i = 0; i < procs.Count && i < 50; i++)
        {
            try
            {
                long mb = procs[i].WorkingSet64 / (1024 * 1024);
                Console.WriteLine($"{i + 1,3}. {procs[i].ProcessName,-25} PID={procs[i].Id,6} RAM={mb,4} MB");
            }
            catch { /* Zugriff verweigert überspringen */ }
        }
        if (procs.Count > 50) Console.WriteLine($"... und {procs.Count - 50} weitere.");
        Console.WriteLine("1) Beenden   2) Prozess killen");
        Console.Write("> ");
        string? k = Console.ReadLine()?.Trim();
        if (k == "2")
        {
            Console.Write("PID: ");
            if (int.TryParse(Console.ReadLine(), out int pid))
            {
                try { Process.GetProcessById(pid).Kill(); Console.WriteLine("✅ Beendet."); }
                catch (Exception ex) { Console.WriteLine("❌ " + ex.Message); }
            }
        }
    }

    private void RunFileBrowser()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ 📁 Datei-Browser             ║");
        Console.WriteLine("╚══════════════════════════════╝");
        string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        bool back = false;
        while (!back)
        {
            try
            {
                Console.WriteLine($"\n📂 {path}");
                var dirs = Directory.GetDirectories(path).OrderBy(d => d).ToArray();
                var files = Directory.GetFiles(path).OrderBy(f => f).ToArray();
                int idx = 1;
                foreach (var d in dirs)
                    Console.WriteLine($"  {idx++,3}. 📁 {Path.GetFileName(d)}");
                foreach (var f in files.Take(30))
                {
                    var fi = new FileInfo(f);
                    Console.WriteLine($"  {idx++,3}. 📄 {Path.GetFileName(f),-30} {fi.Length,10} B");
                }
                if (files.Length > 30) Console.WriteLine($"  ... und {files.Length - 30} weitere Dateien.");
            }
            catch (Exception ex) { Console.WriteLine("❌ " + ex.Message); }

            Console.WriteLine("\n[Pfad eingeben] | [Nummer] = rein | [..] = hoch | [0] = Ende");
            Console.Write("> ");
            string? inp = Console.ReadLine()?.Trim();
            if (inp == "0" || inp == "") { back = true; continue; }
            if (inp == "..")
            {
                var parent = Directory.GetParent(path);
                if (parent != null) path = parent.FullName;
                continue;
            }
            if (Directory.Exists(inp)) { path = inp; continue; }
            if (int.TryParse(inp, out int n))
            {
                try
                {
                    var dirs = Directory.GetDirectories(path).OrderBy(d => d).ToArray();
                    if (n >= 1 && n <= dirs.Length) path = dirs[n - 1];
                }
                catch { }
            }
        }
    }

    private void RunDuplicateFinder()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ 🔍 Duplicate Finder          ║");
        Console.WriteLine("╚══════════════════════════════╝");
        Console.Write("Ordnerpfad: ");
        string dir = Console.ReadLine()?.Trim() ?? "";
        if (!Directory.Exists(dir)) { Console.WriteLine("❌ Ordner nicht gefunden."); return; }
        var files = Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).ToArray();
        Console.WriteLine($"{files.Length} Dateien gefunden — hashes berechnen...");
        var groups = new Dictionary<string, List<string>>();
        foreach (var f in files)
        {
            try
            {
                string hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(f)));
                if (!groups.ContainsKey(hash)) groups[hash] = new List<string>();
                groups[hash].Add(f);
            }
            catch { }
        }
        int dupes = 0;
        foreach (var g in groups.Values.Where(v => v.Count > 1))
        {
            dupes++;
            Console.WriteLine($"\n🔁 Duplikat-Gruppe ({g.Count}x):");
            foreach (var f in g) Console.WriteLine("   " + f);
        }
        Console.WriteLine(dupes == 0 ? "✅ Keine Duplikate gefunden." : $"\n{dupes} Duplikat-Gruppen gefunden.");
    }

    private void RunPortScanner()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ 🔌 Port-Scanner              ║");
        Console.WriteLine("╚══════════════════════════════╝");
        Console.Write("IP/Host: "); string host = Console.ReadLine()?.Trim() ?? "127.0.0.1";
        Console.Write("Ports (z.B. 80,443,3306): "); string ports = Console.ReadLine()?.Trim() ?? "80,443";
        if (!IPAddress.TryParse(host, out IPAddress? ip))
        {
            try { ip = Dns.GetHostAddresses(host).FirstOrDefault(); }
            catch { }
        }
        if (ip == null) { Console.WriteLine("❌ Host nicht auflösbar."); return; }
        foreach (var p in ports.Split(',').Select(s => s.Trim()).Where(s => int.TryParse(s, out _)))
        {
            int port = int.Parse(p);
            try
            {
                using var client = new TcpClient();
                client.ConnectAsync(ip, port).Wait(800);
                Console.WriteLine($"  🟢 Port {port} offen");
            }
            catch { Console.WriteLine($"  🔴 Port {port} geschlossen / filter"); }
        }
    }

    private void RunHttpClientTool()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ 🌐 HTTP-Client               ║");
        Console.WriteLine("╚══════════════════════════════╝");
        Console.WriteLine("1) GET   2) POST   0) Ende");
        Console.Write("> "); string? k = Console.ReadLine()?.Trim();
        if (k == "0") return;
        Console.Write("URL: "); string url = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrEmpty(url)) return;
        using HttpClient c = new();
        c.Timeout = TimeSpan.FromSeconds(15);
        try
        {
            string resp;
            if (k == "2")
            {
                Console.Write("JSON Body: "); string body = Console.ReadLine() ?? "{}";
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                resp = c.PostAsync(url, content).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            else
            {
                resp = c.GetStringAsync(url).GetAwaiter().GetResult();
            }
            if (resp.Length > 2000) resp = resp[..2000] + "\n...(abgeschnitten)";
            try
            {
                var doc = JsonDocument.Parse(resp);
                Console.WriteLine(JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { Console.WriteLine(resp); }
        }
        catch (Exception ex) { Console.WriteLine("❌ " + ex.Message); }
    }

    // ===================== Kreativ & Spiele =====================

    private void RunSnakeGame()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ 🐍 Snake (Pfeiltasten)       ║");
        Console.WriteLine("╚══════════════════════════════╝");
        Console.WriteLine("Beliebige Taste zum Starten...");
        Console.ReadKey(intercept: true);
        int w = Math.Min(40, Console.WindowWidth - 2);
        int h = Math.Min(20, Console.WindowHeight - 4);
        if (w < 10 || h < 5) { Console.WriteLine("Fenster zu klein."); return; }
        var snake = new List<(int x, int y)> { (w / 2, h / 2) };
        int dx = 1, dy = 0;
        var rnd = new Random();
        (int x, int y) food = (rnd.Next(1, w - 1), rnd.Next(1, h - 1));
        int score = 0;
        bool gameOver = false;
        Console.CursorVisible = false;
        while (!gameOver)
        {
            if (Console.KeyAvailable)
            {
                var k = Console.ReadKey(intercept: true).Key;
                if (k == ConsoleKey.UpArrow && dy == 0) { dx = 0; dy = -1; }
                if (k == ConsoleKey.DownArrow && dy == 0) { dx = 0; dy = 1; }
                if (k == ConsoleKey.LeftArrow && dx == 0) { dx = -1; dy = 0; }
                if (k == ConsoleKey.RightArrow && dx == 0) { dx = 1; dy = 0; }
                if (k == ConsoleKey.Escape) break;
            }
            var head = (x: snake[0].x + dx, y: snake[0].y + dy);
            if (head.x <= 0 || head.x >= w - 1 || head.y <= 0 || head.y >= h - 1 || snake.Any(s => s.x == head.x && s.y == head.y))
            { gameOver = true; break; }
            snake.Insert(0, head);
            if (head.x == food.x && head.y == food.y)
            {
                score += 10;
                do { food = (rnd.Next(1, w - 1), rnd.Next(1, h - 1)); } while (snake.Any(s => s.x == food.x && s.y == food.y));
            }
            else { snake.RemoveAt(snake.Count - 1); }

            Console.Clear();
            Console.WriteLine("Score: " + score + "  [Escape = Ende]");
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (x == 0 || x == w - 1 || y == 0 || y == h - 1) Console.Write("#");
                    else if (x == food.x && y == food.y) Console.Write("*");
                    else if (snake.Any(s => s.x == x && s.y == y)) Console.Write("O");
                    else Console.Write(" ");
                }
                Console.WriteLine();
            }
            Thread.Sleep(120);
        }
        Console.WriteLine($"Game Over! Score: {score}");
    }

    private void RunRockPaperScissors()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ ✊ Schere-Stein-Papier       ║");
        Console.WriteLine("╚══════════════════════════════╝");
        string path = Path.Combine(GetSysCoreDataDirectory(), "admin_rps_stats.txt");
        int wins = 0, losses = 0, draws = 0;
        if (File.Exists(path))
        {
            var parts = File.ReadAllText(path).Split(',');
            if (parts.Length == 3) { int.TryParse(parts[0], out wins); int.TryParse(parts[1], out losses); int.TryParse(parts[2], out draws); }
        }
        bool end = false;
        while (!end)
        {
            Console.WriteLine("1) Schere  2) Stein  3) Papier  4) Ende");
            Console.Write("> ");
            string? k = Console.ReadLine()?.Trim();
            if (k == "4") { end = true; continue; }
            if (k != "1" && k != "2" && k != "3") continue;
            string[] names = { "Schere", "Stein", "Papier" };
            int player = int.Parse(k) - 1;
            int cpu = new Random().Next(0, 3);
            Console.WriteLine($"Du: {names[player]}  |  CPU: {names[cpu]}");
            int diff = (player - cpu + 3) % 3;
            if (diff == 0) { Console.WriteLine("🟡 Unentschieden"); draws++; }
            else if (diff == 2) { Console.WriteLine("🟢 Gewonnen!"); wins++; }
            else { Console.WriteLine("🔴 Verloren!"); losses++; }
            int total = wins + losses + draws;
            Console.WriteLine($"Stats: {wins}S / {losses}N / {draws}U  (Winrate: {(total > 0 ? (100.0 * wins / total) : 0):0.0}%)");
        }
        File.WriteAllText(path, $"{wins},{losses},{draws}");
    }

    private void RunRandomGenerator()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ 🎰 Zufallsgenerator          ║");
        Console.WriteLine("╚══════════════════════════════╝");
        Console.WriteLine("1) Passwort  2) UUID  3) Lottozahlen  4) Zahl (min-max)  0) Ende");
        Console.Write("> "); string? k = Console.ReadLine()?.Trim();
        var rnd = new Random();
        switch (k)
        {
            case "1":
                Console.Write("Länge: "); int.TryParse(Console.ReadLine(), out int len);
                if (len < 4) len = 12;
                const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
                Console.WriteLine(new string(Enumerable.Repeat(chars, len).Select(s => s[rnd.Next(s.Length)]).ToArray()));
                break;
            case "2":
                Console.WriteLine(Guid.NewGuid().ToString());
                break;
            case "3":
                var nums = Enumerable.Range(1, 45).OrderBy(_ => rnd.Next()).Take(6).OrderBy(x => x).ToArray();
                Console.WriteLine("Lotto: " + string.Join(", ", nums));
                break;
            case "4":
                Console.Write("Min: "); int.TryParse(Console.ReadLine(), out int min);
                Console.Write("Max: "); int.TryParse(Console.ReadLine(), out int max);
                Console.WriteLine(rnd.Next(min, max + 1));
                break;
        }
    }

    // ===================== BMI / Fitness =====================

    private void RunBmiCalculator()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ ⚖️  BMI / Fitness-Rechner    ║");
        Console.WriteLine("╚══════════════════════════════╝");
        Console.Write("Körpergröße (cm): "); double.TryParse(Console.ReadLine()?.Replace(",", "."), out double h);
        Console.Write("Gewicht (kg): "); double.TryParse(Console.ReadLine()?.Replace(",", "."), out double w);
        if (h <= 0 || w <= 0) { Console.WriteLine("❌ Ungültige Eingabe."); return; }
        double bmi = w / ((h / 100) * (h / 100));
        string label = bmi switch { < 18.5 => "Untergewicht", < 25 => "Normalgewicht", < 30 => "Übergewicht", _ => "Adipositas" };
        Console.WriteLine($"BMI: {bmi:0.0} — {label}");
        Console.Write("Alter: "); int.TryParse(Console.ReadLine(), out int age);
        Console.Write("Geschlecht (m/w): "); string g = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "m";
        double grund = g.StartsWith("w") ? (655 + 9.6 * w + 1.8 * h - 4.7 * age) : (66 + 13.7 * w + 5 * h - 6.8 * age);
        Console.WriteLine($"Geschätzter Grundumsatz: {grund:0} kcal/Tag");
    }
}
