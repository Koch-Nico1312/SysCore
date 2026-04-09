# SysCore

> Ein interaktives Konsolen-Dashboard für Windows — gebaut mit C# / .NET 9  
> **Autor:** Koch Nico

---

## Übersicht

SysCore ist eine vollständige Konsolenanwendung, die nach einem animierten Startbildschirm und einem Login-System je nach Rolle das Admin-Portal oder das Casual-Portal öffnet. Die Oberfläche passt sich dynamisch an die Konsolenbreite an, unterstützt Maus- und Tastaturnavigation und läuft sowohl interaktiv als auch in umgeleiteten Ausgaben (z. B. Pipes).

---

## Features auf einen Blick

| Bereich | Was es macht |
|---|---|
| **Startbildschirm** | Zentrierter Unicode-Titel + ASCII-Banner + Ladebalken (~2,4 s) |
| **Login** | Namenseingabe mit zentrierter Ausgabe; Admin-Erkennung (case-insensitive) |
| **Admin-Portal** | Animiertes Hauptmenü mit Mausunterstützung + Live-Systemleiste |
| **Casual-Portal** | Eigenständiges Menü mit Endnutzer-Tools (Notizen, Rechner, Timer u. v. m.) |
| **19 Admin-Programme** | Siehe Liste unten |

---

## Voraussetzungen

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Windows (empfohlen — System Monitor und Mauseingabe sind Windows-exklusiv)
- Konsolenfenster mit min. ~80 Zeichen Breite für volle Darstellung

---

## Projektstruktur

```
SysCore/
├── Program.cs                      ← Einstiegspunkt (Start → Login → Admin oder Casual)
├── SysCore.csproj
├── SysCore.sln
│
├── Start screen/
│   └── StartScreenView.cs          ← Ladescreen mit Unicode-Titel, ASCII-Banner, Ladebalken
│
├── LoginPage/
│   └── LoginPageView.cs            ← Login-UI mit Admin-Erkennung (Nico / Palitsch / PALI)
│
├── Admin/
│   ├── AdminPortal.cs              ← Kern: Startlogik, Hauptmenü, Programm-Dispatcher
│   ├── AdminPortal.AdminShell.cs   ← Layout, Banner, Systemleiste, Theme-Farben, UI-Update
│   ├── AdminPortal.Menu.cs         ← Menü-Engine (Tastatur + Maus, Zeichnen, Navigation)
│   ├── AdminPortal.Productivity.cs ← Task Manager, Notizbuch, Passwort Manager, Kalender
│   ├── AdminPortal.Storage.cs      ← AppData-Pfade, XOR/SHA-256-Verschlüsselung
│   ├── AdminPortal.SystemCreative.cs← System Monitor, ASCII Art, Farbpalette, Musik Player
│   ├── AdminPortal.Tools.cs        ← Rechner, Caesar, QR, Währung, Datum, Text, Weltzeit,
│   │                                  Würfelsimulator, Quiz, Ausgaben-Tracker
│   └── ConsoleInputWindows.cs      ← Windows-API für Tastatur- und Mauseingabe
│
└── casualUser/
    └── Program.cs                  ← Casual-Portal mit eigenem Menü und Endnutzer-Tools
```

---

## Installation & Start

## Download
[⬇ SysCore.exe herunterladen](https://github.com/Koch-Nico1312/REPO/releases/latest/download/SysCore.exe)
---

## Bedienung

### Navigation im Admin-Menü

| Taste / Aktion | Funktion |
|---|---|
| `↑` / `↓` | Menüpunkt wechseln |
| `Enter` | Auswählen |
| `Escape` | Zurück / Beenden |
| Mausklick | Direkt auswählen |
| Maus-Hover | Markierung folgt der Maus |

### Login

Beim Start wirst du nach deinem Namen gefragt.  
Admin-Namen (case-insensitive): **Nico**, **Palitsch**, **PALI**

- Admin-Name erkannt → Admin-Portal öffnet sich
- Kein Admin-Name → Casual-Portal öffnet sich

---

## Programme im Admin-Portal

### 1. Task Manager
Aufgaben mit Deadlines anlegen, auflisten und als erledigt markieren.  
Speicherort: `%APPDATA%\SysCore\admin_aufgaben.txt`

### 2. Notizbuch
Notizen erstellen, auflisten, anzeigen und per Volltext suchen.  
Speicherort: `%APPDATA%\SysCore\notizen\*.txt`

### 3. Passwort Manager
Einträge (Bezeichnung, Benutzer, Passwort) werden XOR-verschlüsselt mit einem SHA-256-Hash des Master-Passworts gespeichert.  
Speicherort: `%APPDATA%\SysCore\admin_passwort_tresor.txt`

### 4. Kalender
Monatsansicht für beliebiges Jahr und Monat, auf Deutsch (Mo–So).

### 5. Einheitenrechner
Konvertierungen: km↔m, kg↔lb, °C↔°F

### 6. Taschenrechner mit Verlauf
Beliebige mathematische Ausdrücke (z. B. `3+4*2`). `verlauf` zeigt die Historie, `ende` beendet.

### 7. Caesar / Verschlüsselungs-Tool
Text ver- oder entschlüsseln mit beliebiger Verschiebung (1–25).

### 8. Text Analyzer
Gibt Wörter, Zeichen (mit/ohne Leerzeichen) und Sätze eines Textes aus.

### 9. Währungsrechner
Echtzeit-Wechselkurse via [Frankfurter API](https://www.frankfurter.app/).  
Eingabe: Betrag, ISO-Quellwährung (z. B. `EUR`), ISO-Zielwährung (z. B. `USD`).

### 10. QR-Code Generator
Erzeugt einen ASCII-QR-Code für beliebigen Text direkt in der Konsole (via QRCoder-Bibliothek).

### 11. Datumrechner
Berechnet die Anzahl der Tage zwischen zwei Daten (Format: `dd.MM.yyyy`).

### 12. System Monitor *(Windows only)*
Live-Anzeige von CPU-Auslastung (gesamt) und freiem RAM, aktualisiert alle ~700 ms.  
Beenden mit `Escape`, `Q` oder `Enter`.

### 13. ASCII Art Generator
Wandelt Text (A–Z, 0–9, Leerzeichen) in einen 5-zeiligen Block-ASCII-Art-Banner um.

### 14. Farbpaletten Generator
Erzeugt zufällige harmonische Farbpaletten (je 5 Töne) als Hex-Codes direkt in der Konsole.

### 15. Musik Player
Spielt eine lokale MP3-Datei ab (via NAudio). Pfad eingeben → Beliebige Taste stoppt die Wiedergabe.

### 16. Weltzeit-Anzeige
Zeigt die aktuelle Uhrzeit in mehreren Zeitzonen gleichzeitig an.

### 17. Würfelsimulator
Simuliert einen oder mehrere Würfelwürfe mit wählbarer Seitenzahl.

### 18. Quiz
Interaktives Multiple-Choice-Quiz direkt in der Konsole.

### 19. Ausgaben-Tracker
Einnahmen und Ausgaben erfassen und eine Übersicht anzeigen lassen.

---

## Datenspeicherung

Alle persistenten Daten werden unter `%APPDATA%\SysCore\` gespeichert und beim ersten Start automatisch angelegt:

```
%APPDATA%\SysCore\
├── admin_aufgaben.txt          ← Task Manager
├── admin_passwort_tresor.txt   ← Passwort Manager (XOR-verschlüsselt)
└── notizen/
    └── *.txt                   ← Notizbuch-Einträge
```

---

## Abhängigkeiten (NuGet)

| Paket | Zweck |
|---|---|
| `QRCoder` | QR-Code-Generierung |
| `NAudio` | MP3-Wiedergabe (Musik Player) |
| `System.Configuration.ConfigurationManager` | Konfigurationsverwaltung |
| `System.Diagnostics.PerformanceCounter` | CPU/RAM-Auslastung (System Monitor) |

---

## Bekannte Einschränkungen

- System Monitor und Mauseingabe funktionieren nur unter **Windows**
- Währungsrechner benötigt eine aktive **Internetverbindung**
- Musik Player unterstützt derzeit nur **MP3**-Dateien

---

## Lizenz

Privates Schulprojekt — Koch Nico, HTL Leonding, 2025/2026.
