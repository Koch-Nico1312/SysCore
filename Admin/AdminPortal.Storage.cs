namespace AdminApp;

public sealed partial class AdminPortal
{
    private static string SysCoreDatenVerzeichnisErmitteln()
    {
        string basis = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string ordner = Path.Combine(basis, "SysCore");
        Directory.CreateDirectory(ordner);
        return ordner;
    }

    private static string AufgabenDateiPfadBilden()
    {
        return Path.Combine(SysCoreDatenVerzeichnisErmitteln(), "admin_aufgaben.txt");
    }

    private static string NotizenOrdnerPfadBilden()
    {
        string n = Path.Combine(SysCoreDatenVerzeichnisErmitteln(), "notizen");
        Directory.CreateDirectory(n);
        return n;
    }

    private static string PasswortTresorPfadBilden()
    {
        return Path.Combine(SysCoreDatenVerzeichnisErmitteln(), "admin_passwort_tresor.txt");
    }

    private static byte[] MasterSchluesselAusPasswortErzeugen(string masterPasswort)
    {
        return System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(masterPasswort));
    }

    private static string TextMitSchluesselXorVerschleiern(string klartext, byte[] schluessel)
    {
        byte[] p = System.Text.Encoding.UTF8.GetBytes(klartext);
        byte[] o = new byte[p.Length];
        for (int i = 0; i < p.Length; i++)
            o[i] = (byte)(p[i] ^ schluessel[i % schluessel.Length]);

        return Convert.ToBase64String(o);
    }

    private static string TextMitSchluesselXorZurueck(string base64, byte[] schluessel)
    {
        byte[] p = Convert.FromBase64String(base64);
        byte[] o = new byte[p.Length];
        for (int i = 0; i < p.Length; i++)
            o[i] = (byte)(p[i] ^ schluessel[i % schluessel.Length]);

        return System.Text.Encoding.UTF8.GetString(o);
    }
}
