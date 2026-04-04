using System.Globalization;
using System.Text;

namespace AdminApp;

public sealed partial class AdminPortal
{
    private sealed class AufgabeEintrag
    {
        internal string Titel = "";
        internal DateTime Deadline;
        internal bool Erledigt;
    }

    private void TaskManagerAusfuehren()
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
                TaskManagerListeAnzeigen();
            else if (k == "2")
                TaskManagerNeueAufgabeDialog();
            else if (k == "3")
                TaskManagerErledigtMarkierenDialog();
        }
    }

    private void TaskManagerListeAnzeigen()
    {
        List<AufgabeEintrag> liste = AufgabenAusDateiLaden();
        if (liste.Count == 0)
        {
            Console.WriteLine("(keine Aufgaben)");
            return;
        }

        for (int i = 0; i < liste.Count; i++)
        {
            AufgabeEintrag a = liste[i];
            string status = a.Erledigt ? "[x]" : "[ ]";
            Console.WriteLine($"{i + 1}. {status} {a.Titel} — {a.Deadline:yyyy-MM-dd}");
        }
    }

    private void TaskManagerNeueAufgabeDialog()
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

        List<AufgabeEintrag> liste = AufgabenAusDateiLaden();
        liste.Add(new AufgabeEintrag { Titel = titel, Deadline = deadline, Erledigt = false });
        AufgabenInDateiSpeichern(liste);
        Console.WriteLine("Gespeichert.");
    }

    private void TaskManagerErledigtMarkierenDialog()
    {
        List<AufgabeEintrag> liste = AufgabenAusDateiLaden();
        TaskManagerListeAnzeigen();
        Console.Write("Nummer: ");
        string? n = Console.ReadLine();
        if (!int.TryParse(n, out int idx) || idx < 1 || idx > liste.Count)
            return;
        liste[idx - 1].Erledigt = true;
        AufgabenInDateiSpeichern(liste);
    }

    private List<AufgabeEintrag> AufgabenAusDateiLaden()
    {
        List<AufgabeEintrag> liste = [];
        string pfad = AufgabenDateiPfadBilden();
        if (!File.Exists(pfad))
            return liste;

        foreach (string roh in File.ReadAllLines(pfad, Encoding.UTF8))
        {
            string zeile = roh.Trim();
            if (zeile.Length == 0)
                continue;
            AufgabeEintrag? a = AufgabeAusZeileParsen(zeile);
            if (a != null)
                liste.Add(a);
        }

        return liste;
    }

    private static AufgabeEintrag? AufgabeAusZeileParsen(string zeile)
    {
        string[] teile = zeile.Split('|');
        if (teile.Length < 3)
            return null;
        string titel = teile[0];
        if (!DateTime.TryParseExact(teile[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime deadline))
            return null;
        bool erl = teile[2] == "1" || teile[2].Equals("true", StringComparison.OrdinalIgnoreCase);
        return new AufgabeEintrag { Titel = titel, Deadline = deadline, Erledigt = erl };
    }

    private void AufgabenInDateiSpeichern(List<AufgabeEintrag> liste)
    {
        StringBuilder sb = new();
        foreach (AufgabeEintrag a in liste)
        {
            string f = a.Erledigt ? "1" : "0";
            sb.Append(a.Titel.Replace('|', ' ')).Append('|').Append(a.Deadline.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append('|').AppendLine(f);
        }

        File.WriteAllText(AufgabenDateiPfadBilden(), sb.ToString(), Encoding.UTF8);
    }

    private void NotizbuchAusfuehren()
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
                NotizbuchListeAnzeigen();
            else if (k == "2")
                NotizbuchNeuAnlegen();
            else if (k == "3")
                NotizbuchAnzeigenDialog();
            else if (k == "4")
                NotizbuchSuchenDialog();
        }
    }

    private void NotizbuchListeAnzeigen()
    {
        string ordner = NotizenOrdnerPfadBilden();
        string[] dateien = Directory.GetFiles(ordner, "*.txt", SearchOption.TopDirectoryOnly);
        if (dateien.Length == 0)
        {
            Console.WriteLine("(keine Notizen)");
            return;
        }

        foreach (string pfad in dateien)
            Console.WriteLine(Path.GetFileNameWithoutExtension(pfad));
    }

    private void NotizbuchNeuAnlegen()
    {
        Console.Write("Titel (Dateiname): ");
        string? t = Console.ReadLine();
        string titel = NotizbuchDateinameBereinigen(t?.Trim() ?? "");
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

        string pfad = Path.Combine(NotizenOrdnerPfadBilden(), titel + ".txt");
        File.WriteAllText(pfad, inhalt.ToString(), Encoding.UTF8);
        Console.WriteLine("Gespeichert.");
    }

    private static string NotizbuchDateinameBereinigen(string roh)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            roh = roh.Replace(c, '_');
        return roh.Trim();
    }

    private void NotizbuchAnzeigenDialog()
    {
        Console.Write("Titel: ");
        string? t = Console.ReadLine();
        string titel = NotizbuchDateinameBereinigen(t?.Trim() ?? "");
        if (titel.Length == 0)
            return;
        string pfad = Path.Combine(NotizenOrdnerPfadBilden(), titel + ".txt");
        if (!File.Exists(pfad))
        {
            Console.WriteLine("Nicht gefunden.");
            return;
        }

        Console.WriteLine("---");
        Console.WriteLine(File.ReadAllText(pfad, Encoding.UTF8));
        Console.WriteLine("---");
    }

    private void NotizbuchSuchenDialog()
    {
        Console.Write("Suchbegriff: ");
        string? q = Console.ReadLine();
        string needle = q?.Trim() ?? "";
        if (needle.Length == 0)
            return;
        string ordner = NotizenOrdnerPfadBilden();
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

    private void PasswortManagerAusfuehren()
    {
        Console.WriteLine("=== Passwort Manager (XOR + SHA256) ===");
        Console.Write("Master-Passwort: ");
        string? m = Console.ReadLine();
        string master = m ?? "";
        if (master.Length == 0)
            return;
        byte[] schluessel = MasterSchluesselAusPasswortErzeugen(master);
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
                PasswortManagerListeAnzeigen();
            else if (k == "2")
                PasswortManagerEintragHinzufuegen(schluessel);
            else if (k == "3")
                PasswortManagerEintragAnzeigen(schluessel);
        }
    }

    private void PasswortManagerListeAnzeigen()
    {
        string pfad = PasswortTresorPfadBilden();
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

    private void PasswortManagerEintragHinzufuegen(byte[] schluessel)
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
        string enc = TextMitSchluesselXorVerschleiern(nutzlast, schluessel);
        string zeile = bez.Replace('|', ' ') + "|" + enc;
        File.AppendAllText(PasswortTresorPfadBilden(), zeile + Environment.NewLine, Encoding.UTF8);
        Console.WriteLine("Eintrag gespeichert.");
    }

    private void PasswortManagerEintragAnzeigen(byte[] schluessel)
    {
        Console.Write("Bezeichnung: ");
        string? b = Console.ReadLine();
        string bez = b?.Trim() ?? "";
        if (bez.Length == 0)
            return;
        string pfad = PasswortTresorPfadBilden();
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
            string klar = TextMitSchluesselXorZurueck(enc, schluessel);
            string[] teile = klar.Split('\n');
            string user = teile.Length > 0 ? teile[0] : "";
            string pass = teile.Length > 1 ? teile[1] : "";
            Console.WriteLine("Benutzer: " + user);
            Console.WriteLine("Passwort: " + pass);
            return;
        }

        Console.WriteLine("Nicht gefunden.");
    }

    private void KalenderAusfuehren()
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

        KalenderMonatZeichnen(jahr, monat);
    }

    private static void KalenderMonatZeichnen(int jahr, int monat)
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
