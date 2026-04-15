using System.Text;
using System.Text.Json;

namespace AdminApp;

public sealed partial class AdminPortal
{
    // Kleine Helfer zum sicheren Datei lesen.
    private static string? ReadTextFileSafe(string path)
    {
        try
        {
            using StreamReader r = new(path, Encoding.UTF8);
            return r.ReadToEnd();
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Datei nicht gefunden: " + path);
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Kein Zugriff auf Datei.");
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine("fehler: " + e.Message);
            return null;
        }
    }

    private static T? ReadJsonFileSafe<T>(string path) where T : class
    {
        try
        {
            using StreamReader r = new(path, Encoding.UTF8);
            string json = r.ReadToEnd();
            T? data = JsonSerializer.Deserialize<T>(json);
            return data;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("JSON-Datei fehlt: " + path);
            return null;
        }
        catch (JsonException ex)
        {
            Console.WriteLine("JSON ungültig: " + ex.Message);
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Keine Rechte auf JSON-Datei.");
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine("fehler: " + e.Message);
            return null;
        }
    }
}
