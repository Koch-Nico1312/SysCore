namespace AdminApp;

public sealed partial class AdminPortal
{
    // Ermittelt den App-Datenordner und legt ihn bei Bedarf an.
    private static string GetSysCoreDataDirectory()
    {
        string basis = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string ordner = Path.Combine(basis, "SysCore");
        Directory.CreateDirectory(ordner);
        return ordner;
    }

    // Pfad für die Aufgaben-Datei.
    private static string BuildTasksFilePath()
    {
        return Path.Combine(GetSysCoreDataDirectory(), "admin_aufgaben.txt");
    }

    // Pfad für den Notizen-Ordner.
    private static string BuildNotesFolderPath()
    {
        string n = Path.Combine(GetSysCoreDataDirectory(), "notizen");
        Directory.CreateDirectory(n);
        return n;
    }

    // Pfad für den Passwort-Tresor.
    private static string BuildPasswordVaultPath()
    {
        return Path.Combine(GetSysCoreDataDirectory(), "admin_passwort_tresor.txt");
    }

    // Erzeugt aus dem Master-Passwort einen festen Schlüssel (SHA256).
    private static byte[] CreateMasterKeyFromPassword(string masterPassword)
    {
        return System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(masterPassword));
    }

    // Sehr einfache XOR-"Verschleierung" für gespeicherte Textdaten.
    // Hinweis: Das ist kein starker Schutz wie moderne Verschlüsselung.
    private static string ObfuscateTextWithXorKey(string plainText, byte[] key)
    {
        byte[] p = System.Text.Encoding.UTF8.GetBytes(plainText);
        byte[] o = new byte[p.Length];
        for (int i = 0; i < p.Length; i++)
            o[i] = (byte)(p[i] ^ key[i % key.Length]);

        return Convert.ToBase64String(o);
    }

    // Hebt die XOR-Verschleierung wieder auf.
    private static string RestoreTextWithXorKey(string base64, byte[] key)
    {
        byte[] p = Convert.FromBase64String(base64);
        byte[] o = new byte[p.Length];
        for (int i = 0; i < p.Length; i++)
            o[i] = (byte)(p[i] ^ key[i % key.Length]);

        return System.Text.Encoding.UTF8.GetString(o);
    }
}

// Was macht diese Datei?
// - Bündelt Dateipfade für Admin-Daten.
// - Enthält Hilfsmethoden zum Speichern/Laden verschleierter Passwort-Inhalte.
