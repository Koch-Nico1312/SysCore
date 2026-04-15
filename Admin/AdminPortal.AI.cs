using System.Text.Json;

namespace AdminApp;

public sealed partial class AdminPortal
{
    private const string FreeGeminiModel = "gemini-2.0-flash";

    // Einfaches Config-Objekt für API-Key + Modell.
    private sealed record GeminiConfig(string gemini_api_key, string gemini_model);

    private void RunGeminiChatModule()
    {
        GeminiConfig cfg = LoadOrCreateGeminiConfig();
        using HttpClient c = new();
        c.Timeout = TimeSpan.FromSeconds(40);
        AiService svc = new(c, cfg.gemini_api_key, cfg.gemini_model);
        List<Message> history = [];

        bool done = false;
        while (!done)
        {
            Console.WriteLine("╔══════════════════════════════════╗");
            Console.WriteLine($"║   🤖 SysCore AI  [{cfg.gemini_model}]");
            Console.WriteLine("╚══════════════════════════════════╝");
            Console.Write("Du: ");
            string? inp = Console.ReadLine();
            string msg = inp?.Trim() ?? "";
            if (msg.Length == 0) continue;

            if (msg.Equals("exit", StringComparison.OrdinalIgnoreCase) || msg.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                done = true;
                continue;
            }

            if (msg.Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                history.Clear();
                Console.WriteLine("Verlauf geleert.");
                continue;
            }

            string ans = svc.SendMessageAsync(msg, history).GetAwaiter().GetResult();
            if (ans == "__RATE_LIMIT__")
            {
                Console.WriteLine("Rate Limit erreicht, kurz warten...");
                Thread.Sleep(TimeSpan.FromSeconds(10));
                ans = svc.SendMessageAsync(msg, history).GetAwaiter().GetResult();
            }

            if (ans == "__BAD_KEY__")
            {
                Console.WriteLine("Ungültiger oder abgelaufener API-Key.");
                Console.Write("Neuen Key jetzt eingeben? (j/n): ");
                string? x = Console.ReadLine();
                if ((x ?? "").Trim().Equals("j", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Write("Key: ");
                    string? nk = Console.ReadLine();
                    string key = nk?.Trim() ?? "";
                    if (key.Length > 0)
                    {
                        cfg = cfg with { gemini_api_key = key };
                        SaveGeminiConfig(cfg);
                        svc.UpdateSettings(cfg.gemini_api_key, cfg.gemini_model);
                    }
                }
                continue;
            }

            Console.WriteLine();
            Console.WriteLine("AI: " + ans);
            Console.WriteLine();
            history.Add(new Message("user", msg));
            history.Add(new Message("model", ans));
            // Console.WriteLine("test123");
        }
    }

    private GeminiConfig LoadOrCreateGeminiConfig()
    {
        string path = BuildConfigFilePath();
        GeminiConfig? cfg = ReadJsonFileSafe<GeminiConfig>(path);
        if (cfg == null)
            cfg = new GeminiConfig("", FreeGeminiModel);

        if (string.IsNullOrWhiteSpace(cfg.gemini_api_key))
        {
            Console.WriteLine("Kein Gemini API-Key gefunden. Bitte jetzt eingeben:");
            Console.WriteLine("Einen kostenlosen Key bekommst du auf: https://aistudio.google.com");
            Console.Write("Key: ");
            string? k = Console.ReadLine();
            string api = k?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(api))
                api = "";
            cfg = cfg with { gemini_api_key = api };
        }

        // Admin AI bleibt absichtlich auf dem kostenlosen Modell.
        cfg = cfg with { gemini_model = FreeGeminiModel };

        SaveGeminiConfig(cfg);
        return cfg;
    }

    private static void SaveGeminiConfig(GeminiConfig cfg)
    {
        string path = BuildConfigFilePath();
        var opts = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(cfg, opts);
        File.WriteAllText(path, json);
    }
}
