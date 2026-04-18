# API Entwicklung mit ASP.NET Core

> [!NOTE]
> Im Ordner [Eventmanager](Eventmanager) befindet sich die fertige Applikation mit den gezeigten Beispielen.
> Klone das Repo mit `git clone https://github.com/Die-Spengergasse/course-pos-csharp_asp` und starte die sln Datei.
> .NET 10 ist erforderlich.

> [!NOTE]
> Diesen Kurs gibt es auch als **PDF** zum offline Nachlesen und als Unterlage für die Prüfung:
> [🖺 Download](https://github.com/Die-Spengergasse/course-pos-csharp_asp/raw/refs/heads/main/asp_book.pdf)

## Inhalt

Mit ○ gekennzeichnete Kapitel sind Erweiterungskapitel.

### Controller und Services in ASP.NET Core

- [RESTful APIs](./01_Rest/README.adoc)
- [Die erste ASP.NET Core App](./02_ASP_Intro/README.adoc)
- [Controller in ASP.NET Core: GET Routen](./03_Get_Routes/README.adoc)
- [POST Routen, Services und Validierung](./04_Post_Routes/README.adoc)
- [PUT, PATCH und DELETE](./05_Put_Patch_Delete_Routes/README.adoc)

### Testen

- [Servicetests mit der SQLite in-memory DB](./10_Service_Tests/README.adoc)
- [Integration Tests mit ASP.NET Core](./11_Integration_Tests/README.adoc)
- ○ [Mocking mit NSubstitute](./12_Mocking/README.adoc)
- ○ [Das Repository Pattern](./13_Repository/README.adoc)

### Erweiterte Techniken

- ○ [Encoding von ID Werten](./20_Id_Encoder/README.adoc)
- ○ [HATEOAS](./21_Hypermedia/README.adoc)
- ○ [GraphQL](./22_GraphQL/README.adoc)
- ○ [Minimal API](./23_MinimalApi/README.adoc)

### Übungen

- [Übungsprüfung zu Services und RESTful API: Sprachwochenverwaltung](./Uebungen/01_Languageweek/README.adoc)
- [PLF vom 15.4.2026 zu Services und RESTful API: Benzinpreise](./Uebungen/02_GasManager/Angabe.adoc)

### Installation von Visual Studio 2026

Lade von https://visualstudio.microsoft.com/de/downloads/ die *Enterprise* Version von Visual Studio 2026 herunter.
Falls du unter macOS arbeitest, stelle sicher, dass du die neueste Rider Installation besitzt, die .NET 10 unterstützt.

Wähle im Installer den Workload *ASP.NET* and web development_ aus.
Wenn du das Feature der [AOT Kompilierung](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot) nutzen möchtest, musst du _Desktop development with C++_ aktivieren.
Installiere unter *Individual options* zusätzlich die .NET 8 Runtime, um ältere Projekte ausführen zu können.
Unter *Language packs* wähle das englische Sprachpaket.

![](vs2026_installer_0856.png)

Starte nach der Installation Visual Studio 2026.
Du kannst mit *Help* - *Register Visual Studio* den Key eingeben.

![](vs2026_activate_2041.png)
