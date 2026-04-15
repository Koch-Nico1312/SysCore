using System.ComponentModel;
using System.Diagnostics;

namespace AdminApp;

public sealed partial class AdminPortal
{
    private void RunExternalProgramLauncher()
    {
        Console.WriteLine("╔══════════════════════════════╗");
        Console.WriteLine("║ 🚀 Programmstarter           ║");
        Console.WriteLine("╚══════════════════════════════╝");
        bool fertig = false;
        while (!fertig)
        {
            Console.WriteLine("1) Notepad");
            Console.WriteLine("2) Explorer");
            Console.WriteLine("3) Eigene exe starten");
            Console.WriteLine("4) Zurück");
            Console.Write("> ");
            var x = Console.ReadLine();
            string k = x?.Trim() ?? "";
            if (k == "4") { fertig = true; continue; }

            if (k == "1")
                TryStartProgram("notepad.exe", null);
            else if (k == "2")
                TryStartProgram("explorer.exe", null);
            else if (k == "3")
            {
                Console.Write("Pfad zur exe: ");
                string? p = Console.ReadLine();
                string path = p?.Trim() ?? "";
                if (path.Length == 0) continue;
                TryStartProgram(path, null);
            }
        }
    }

    private void TryStartProgram(string file, string? args)
    {
        try
        {
            ProcessStartInfo psi = new();
            psi.FileName = file;
            if (!string.IsNullOrWhiteSpace(args))
                psi.Arguments = args;
            psi.UseShellExecute = true;
            Process.Start(psi);
            Console.WriteLine("✅ gestartet: " + file);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("❌ Programm nicht gefunden.");
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("❌ Zugriff verweigert.");
        }
        catch (Win32Exception ex)
        {
            Console.WriteLine("❌ Startfehler: " + ex.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine("fehler: " + e.Message);
        }
    }
}
