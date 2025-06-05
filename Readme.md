## OpenKNX.Toolbox

Die OpenKNX Toolbox stellt einen einfachn Weg zur Verfügung,  
um OpenKNX Geräte mit Firmware zu versorgen und die KnxProd zu erstellen.  
  
Die verfügbaren Repos werden lokal gespeichert und können jederzeit manuell aktualisiert werden.  
>Das RateLimit von 100 Github zugriffen wird jede Stunde zurückgesetzt.

Aktuell unterstütz werden folgende Prozessoren:  
 - RP2040 (als Massenspeichergerät, als auch Seriellesgerät)
 - ESP32 (noch nicht öffentlich)

### Unterstütze Betriebssysteme
Die Toolbox funktioniert auf Windows und Linux PCs.  
Der Linux Support ist jedoch noch experimentell und   
nicht alle funktionen werden unterstützt.

### Benennung der Releases in den Repos
Der Tag des Release muss dem SematicVersion entsprechen:  
`major.minor.build-buildLabel+patchLabel`  
Sollte für ein Release mehrere Assets zur Verfügung stehen, werden diese als getrennte Elemente in der Combobox dargestellt. Damit man die Versionen unterscheiden kann, ist es möglich das BuildLabel aus der Version zu Überschreiben. Dies erfolgt über den Dateinamen der Zip:  
`[RepoName]-[BuildLabel]-[SemanticVersion].zip`  
Der Standardwert ist Release oder Beta und wird von den Buildscripten erstellt.  
Das BuildLabel kann beliebig gewählt werden, darf aber kein Bindestrich enthalten.  

### Verwendete Libraries
Dieses Tool verwendet folgende Repos:  
 - [OpenKNX.Toolbox.Lib](https://github.com/OpenKNX/OpenKNX.Toolbox.Lib) GPLv3
 - [OpenKNX.Toolbox.Sign](https://github.com/OpenKNX/OpenKNX.Toolbox.Sign) GPLv3
und folgende Pakete:  
 - [Octokit.net](https://github.com/octokit/octokit.net) MIT
 - [Avalonia](https://www.nuget.org/packages/Avalonia) MIT
 - [Avalonia.Controls.ItemsRepeater](https://www.nuget.org/packages/Avalonia.Controls.ItemsRepeater) MIT
 - [Avalonia.Desktop](https://www.nuget.org/packages/Avalonia.Desktop) MIT
 - [Avalonia.Themes.Fluent](https://www.nuget.org/packages/Avalonia.Themes.Fluent) MIT
 - [MessageBox.Avalonia](https://www.nuget.org/packages/MessageBox.Avalonia) MIT
 - [CommunityToolkit.Mvvm](https://www.nuget.org/packages/CommunityToolkit.Mvvm) MIT
 - [Newtonsoft.JSON](https://www.nuget.org/packages/Newtonsoft.JSON) MIT