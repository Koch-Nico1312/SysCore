using System.Globalization;
using System.Text;

namespace AdminApp;

public sealed partial class AdminPortal
{
    // Kleine Datenklasse für eine Aufgabe.
    private sealed class TaskEntry
    {
        internal string Titel = "";
        internal DateTime Deadline;
        internal bool Erledigt;
    }

    // Hauptmenü vom Task-Manager.
    private void RunTaskManager()
    {
        Console.WriteLine("=== Task Manager ===");
        bool fertig = false;
        while (!fertig)
        {
            Console.WriteLine("1) Liste  2) Neu  3) Erledigt markieren  4) Beenden");
            Console.Write("> ");
            string? w = Console.ReadLine();
            string k = w?.Trim() ?? "";
            if (k == "4")
            {
                fertig = true;
                continue;
            }

            if (k == "1")
                ShowTaskManagerList();
            else if (k == "2")
                ShowTaskManagerNewTaskDialog();
            else if (k == "3")
                ShowTaskManagerMarkDoneDialog();
        }
    }

    // Zeigt alle Aufgaben mit Status an.
    private void ShowTaskManagerList()
    {
        List<TaskEntry> liste = LoadTasksFromFile();
        if (liste.Count == 0)
        {
            Console.WriteLine("(keine Aufgaben)");
            return;
        }

        for (int i = 0; i < liste.Count; i++)
        {
            TaskEntry a = liste[i];
            string status = a.Erledigt ? "[x]" : "[ ]";
            Console.WriteLine($"{i + 1}. {status} {a.Titel} — {a.Deadline:yyyy-MM-dd}");
        }
    }

    // Fragt Titel und Deadline ab und speichert die neue Aufgabe.
    private void ShowTaskManagerNewTaskDialog()
    {
        Console.Write("Titel: ");
        string? t = Console.ReadLine();
        string titel = t?.Trim() ?? "";
        if (titel.Length == 0)
            return;
        Console.Write("Deadline (yyyy-MM-dd): ");
        string? d = Console.ReadLine();
        string ds = d?.Trim() ?? "";
        if (!DateTime.TryParseExact(ds, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime deadline))
        {
            Console.WriteLine("Ungültiges Datum.");
            return;
        }

        List<TaskEntry> liste = LoadTasksFromFile();
        liste.Add(new TaskEntry { Titel = titel, Deadline = deadline, Erledigt = false });
        SaveTasksToFile(liste);
        Console.WriteLine("Gespeichert.");
    }

    // Markiert eine Aufgabe als erledigt.
    private void ShowTaskManagerMarkDoneDialog()
    {
        List<TaskEntry> liste = LoadTasksFromFile();
        ShowTaskManagerList();
        Console.Write("Nummer: ");
        string? n = Console.ReadLine();
        if (!int.TryParse(n, out int idx) || idx < 1 || idx > liste.Count)
            return;
        liste[idx - 1].Erledigt = true;
        SaveTasksToFile(liste);
    }

    // Liest Aufgaben aus der Datei.
    private List<TaskEntry> LoadTasksFromFile()
    {
        List<TaskEntry> liste = [];
        string pfad = BuildTasksFilePath();
        if (!File.Exists(pfad))
            return liste;

        foreach (string roh in File.ReadAllLines(pfad, Encoding.UTF8))
        {
            string zeile = roh.Trim();
            if (zeile.Length == 0)
                continue;
            TaskEntry? a = ParseTaskFromLine(zeile);
            if (a != null)
                liste.Add(a);
        }

        return liste;
    }

    // Wandelt eine Datei-Zeile in ein Aufgaben-Objekt um.
    private static TaskEntry? ParseTaskFromLine(string zeile)
    {
        string[] teile = zeile.Split('|');
        if (teile.Length < 3)
            return null;
        string titel = teile[0];
        if (!DateTime.TryParseExact(teile[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime deadline))
            return null;
        bool erl = teile[2] == "1" || teile[2].Equals("true", StringComparison.OrdinalIgnoreCase);
        return new TaskEntry { Titel = titel, Deadline = deadline, Erledigt = erl };
    }

    // Schreibt die Aufgabenliste wieder in die Datei.
    private void SaveTasksToFile(List<TaskEntry> liste)
    {
        StringBuilder sb = new();
        foreach (TaskEntry a in liste)
        {
            string f = a.Erledigt ? "1" : "0";
            sb.Append(a.Titel.Replace('|', ' ')).Append('|').Append(a.Deadline.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append('|').AppendLine(f);
        }

        File.WriteAllText(BuildTasksFilePath(), sb.ToString(), Encoding.UTF8);
    }

    // Hauptmenü vom Notizbuch.
    private void RunNotebook()
    {
        Console.WriteLine("=== Notizbuch ===");
        bool fertig = false;
        while (!fertig)
        {
            Console.WriteLine("1) Liste  2) Neu  3) Anzeigen  4) Suchen  5) Beenden");
            Console.Write("> ");
            string? w = Console.ReadLine();
            string k = w?.Trim() ?? "";
            if (k == "5")
            {
                fertig = true;
                continue;
            }

            if (k == "1")
                ShowNotebookList();
            else if (k == "2")
                CreateNewNotebookNote();
            else if (k == "3")
                ShowNotebookNoteDialog();
            else if (k == "4")
                ShowNotebookSearchDialog();
        }
    }

    // Zeigt alle Notiz-Dateien an.
    private void ShowNotebookList()
    {
        string ordner = BuildNotesFolderPath();
        string[] dateien = Directory.GetFiles(ordner, "*.txt", SearchOption.TopDirectoryOnly);
        if (dateien.Length == 0)
        {
            Console.WriteLine("(keine Notizen)");
            return;
        }

        foreach (string pfad in dateien)
            Console.WriteLine(Path.GetFileNameWithoutExtension(pfad));
    }

    // Erstellt eine neue Notizdatei.
    private void CreateNewNotebookNote()
    {
        Console.Write("Titel (Dateiname): ");
        string? t = Console.ReadLine();
        string titel = SanitizeNotebookFileName(t?.Trim() ?? "");
        if (titel.Length == 0)
            return;
        Console.WriteLine("Text (leere Zeile beendet):");
        StringBuilder inhalt = new();
        while (true)
        {
            string? zeile = Console.ReadLine();
            if (zeile == null)
                break;
            if (zeile.Length == 0)
                break;
            inhalt.AppendLine(zeile);
        }

        string pfad = Path.Combine(BuildNotesFolderPath(), titel + ".txt");
        File.WriteAllText(pfad, inhalt.ToString(), Encoding.UTF8);
        Console.WriteLine("Gespeichert.");
    }

    // Macht einen sicheren Dateinamen aus Benutzereingabe.
    private static string SanitizeNotebookFileName(string raw)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            raw = raw.Replace(c, '_');
        return raw.Trim();
    }

    // Zeigt eine vorhandene Notiz komplett an.
    private void ShowNotebookNoteDialog()
    {
        Console.Write("Titel: ");
        string? t = Console.ReadLine();
        string titel = SanitizeNotebookFileName(t?.Trim() ?? "");
        if (titel.Length == 0)
            return;
        string pfad = Path.Combine(BuildNotesFolderPath(), titel + ".txt");
        if (!File.Exists(pfad))
        {
            Console.WriteLine("Nicht gefunden.");
            return;
        }

        Console.WriteLine("---");
        Console.WriteLine(File.ReadAllText(pfad, Encoding.UTF8));
        Console.WriteLine("---");
    }

    // Sucht in Notiznamen und Notizinhalten.
    private void ShowNotebookSearchDialog()
    {
        Console.Write("Suchbegriff: ");
        string? q = Console.ReadLine();
        string needle = q?.Trim() ?? "";
        if (needle.Length == 0)
            return;
        string ordner = BuildNotesFolderPath();
        string[] dateien = Directory.GetFiles(ordner, "*.txt", SearchOption.TopDirectoryOnly);
        bool fund = false;
        foreach (string pfad in dateien)
        {
            string name = Path.GetFileNameWithoutExtension(pfad);
            string text = File.ReadAllText(pfad, Encoding.UTF8);
            if (name.Contains(needle, StringComparison.OrdinalIgnoreCase) || text.Contains(needle, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(Path.GetFileNameWithoutExtension(pfad));
                fund = true;
            }
        }

        if (!fund)
            Console.WriteLine("(keine Treffer)");
    }

    // Hauptmenü vom Passwort-Manager.
    private void RunPasswordManager()
    {
        Console.WriteLine("=== Passwort Manager (XOR + SHA256) ===");
        Console.Write("Master-Passwort: ");
        string? m = Console.ReadLine();
        string master = m ?? "";
        if (master.Length == 0)
            return;
        byte[] schluessel = CreateMasterKeyFromPassword(master);
        bool fertig = false;
        while (!fertig)
        {
            Console.WriteLine("1) Liste  2) Neu  3) Anzeigen  4) Beenden");
            Console.Write("> ");
            string? w = Console.ReadLine();
            string k = w?.Trim() ?? "";
            if (k == "4")
            {
                fertig = true;
                continue;
            }

            if (k == "1")
                ShowPasswordManagerList();
            else if (k == "2")
                AddPasswordManagerEntry(schluessel);
            else if (k == "3")
                ShowPasswordManagerEntry(schluessel);
        }
    }

    // Listet alle gespeicherten Einträge (nur Namen) auf.
    private void ShowPasswordManagerList()
    {
        string pfad = BuildPasswordVaultPath();
        if (!File.Exists(pfad))
        {
            Console.WriteLine("(leer)");
            return;
        }

        foreach (string roh in File.ReadAllLines(pfad, Encoding.UTF8))
        {
            string zeile = roh.Trim();
            if (zeile.Length == 0)
                continue;
            string[] teile = zeile.Split('|');
            if (teile.Length >= 1)
                Console.WriteLine(teile[0]);
        }
    }

    // Fügt einen neuen Passwort-Eintrag hinzu.
    private void AddPasswordManagerEntry(byte[] schluessel)
    {
        Console.Write("Bezeichnung: ");
        string? b = Console.ReadLine();
        string bez = b?.Trim() ?? "";
        if (bez.Length == 0)
            return;
        Console.Write("Benutzer: ");
        string? u = Console.ReadLine();
        string user = u?.Trim() ?? "";
        Console.Write("Passwort: ");
        string? p = Console.ReadLine();
        string pass = p ?? "";
        string nutzlast = user + "\n" + pass;
        string enc = ObfuscateTextWithXorKey(nutzlast, schluessel);
        string zeile = bez.Replace('|', ' ') + "|" + enc;
        File.AppendAllText(BuildPasswordVaultPath(), zeile + Environment.NewLine, Encoding.UTF8);
        Console.WriteLine("Eintrag gespeichert.");
    }

    // Zeigt Benutzername und Passwort zu einem Eintrag an.
    private void ShowPasswordManagerEntry(byte[] schluessel)
    {
        Console.Write("Bezeichnung: ");
        string? b = Console.ReadLine();
        string bez = b?.Trim() ?? "";
        if (bez.Length == 0)
            return;
        string pfad = BuildPasswordVaultPath();
        if (!File.Exists(pfad))
            return;

        foreach (string roh in File.ReadAllLines(pfad, Encoding.UTF8))
        {
            string zeile = roh.Trim();
            if (zeile.Length == 0)
                continue;
            int pipe = zeile.IndexOf('|');
            if (pipe <= 0)
                continue;
            string name = zeile[..pipe];
            if (!name.Equals(bez, StringComparison.OrdinalIgnoreCase))
                continue;
            string enc = zeile[(pipe + 1)..];
            string klar = RestoreTextWithXorKey(enc, schluessel);
            string[] teile = klar.Split('\n');
            string user = teile.Length > 0 ? teile[0] : "";
            string pass = teile.Length > 1 ? teile[1] : "";
            Console.WriteLine("Benutzer: " + user);
            Console.WriteLine("Passwort: " + pass);
            return;
        }

        Console.WriteLine("Nicht gefunden.");
    }

    // Fragt Jahr/Monat ab und zeichnet Monatsansicht.
    private void RunCalendar()
    {
        Console.WriteLine("=== Kalender (Monatsansicht) ===");
        Console.Write("Jahr (Enter = jetzt): ");
        string? y = Console.ReadLine();
        Console.Write("Monat 1-12 (Enter = jetzt): ");
        string? m = Console.ReadLine();
        DateTime jetzt = DateTime.Now;
        int jahr = jetzt.Year;
        int monat = jetzt.Month;
        if (int.TryParse(y, out int jv) && jv >= 1 && jv <= 9999)
            jahr = jv;
        if (int.TryParse(m, out int mv) && mv is >= 1 and <= 12)
            monat = mv;

        DrawCalendarMonth(jahr, monat);
    }

    // Zeichnet den Kalender für einen Monat.
    private static void DrawCalendarMonth(int jahr, int monat)
    {
        CultureInfo de = CultureInfo.GetCultureInfo("de-DE");
        string titel = new DateTime(jahr, monat, 1).ToString("MMMM yyyy", de);
        Console.WriteLine(titel);
        Console.WriteLine("Mo Di Mi Do Fr Sa So");
        DateTime erster = new(jahr, monat, 1);
        int offset = ((int)erster.DayOfWeek + 6) % 7;
        int tage = DateTime.DaysInMonth(jahr, monat);
        int zelle = 0;
        StringBuilder zeile = new();
        for (int i = 0; i < offset; i++)
        {
            zeile.Append("   ");
            zelle++;
        }

        for (int tag = 1; tag <= tage; tag++)
        {
            zeile.Append(tag.ToString(CultureInfo.InvariantCulture).PadLeft(2)).Append(' ');
            zelle++;
            if (zelle % 7 == 0)
            {
                Console.WriteLine(zeile.ToString().TrimEnd());
                zeile.Clear();
            }
        }

        if (zeile.Length > 0)
            Console.WriteLine(zeile.ToString().TrimEnd());
    }
}

// Was macht diese Datei?
// - Enthält Produktivitäts-Tools: Task-Manager, Notizbuch, Passwort-Manager, Kalender.
// - Kapselt Dialoge, Dateioperationen und Ausgabe in der Konsole.
