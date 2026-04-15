using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace AdminApp;

public sealed class AiService
{
    private readonly HttpClient _httpClient;
    private string _apiKey;
    private string _model;

    public AiService(HttpClient httpClient, string apiKey, string model)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
        _model = model;
    }

    public void UpdateSettings(string apiKey, string model)
    {
        _apiKey = apiKey;
        _model = model;
    }

    public async Task<string> SendMessageAsync(string prompt, List<Message> history)
    {
        // TODO: das muss ich noch schöner machen
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
        var contents = new List<object>();
        foreach (var m in history)
        {
            contents.Add(new
            {
                role = m.Role,
                parts = new[] { new { text = m.Text } }
            });
        }
        contents.Add(new
        {
            role = "user",
            parts = new[] { new { text = prompt } }
        });

        string payload = JsonSerializer.Serialize(new { contents });
        using HttpRequestMessage req = new(HttpMethod.Post, url);
        req.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        HttpResponseMessage resp;
        try
        {
            resp = await _httpClient.SendAsync(req);
        }
        catch (HttpRequestException ex)
        {
            return "Kein Internet / API nicht erreichbar: " + ex.Message;
        }
        catch (TaskCanceledException)
        {
            return "API Timeout erreicht.";
        }

        if (resp.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            return "__RATE_LIMIT__";
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized || resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
            return "__BAD_KEY__";

        string body = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            return "API Fehler: " + (int)resp.StatusCode + " " + body;

        try
        {
            using JsonDocument doc = JsonDocument.Parse(body);
            JsonElement root = doc.RootElement;
            if (!root.TryGetProperty("candidates", out JsonElement candidates))
                return "Antwort JSON ungültig: candidates fehlt";
            if (candidates.GetArrayLength() == 0)
                return "Leere Antwort vom Modell.";
            JsonElement c0 = candidates[0];
            if (!c0.TryGetProperty("content", out JsonElement content))
                return "Antwort JSON ungültig: content fehlt";
            if (!content.TryGetProperty("parts", out JsonElement parts))
                return "Antwort JSON ungültig: parts fehlt";
            if (parts.GetArrayLength() == 0)
                return "Antwort enthält keinen Text.";
            JsonElement p0 = parts[0];
            if (!p0.TryGetProperty("text", out JsonElement txt))
                return "Antwort JSON ungültig: text fehlt";
            return txt.GetString() ?? "";
        }
        catch (JsonException ex)
        {
            return "Ungültiges JSON in der Antwort: " + ex.Message;
        }
    }
}

public sealed record Message(string Role, string Text);
