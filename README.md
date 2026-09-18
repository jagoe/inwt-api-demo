# INWT API Demo

This app is a small demo of what an API endpoint for streaming service predictions might look like.

## Run

### Docker Compose

```bash
docker compose up
```

### `dotnet`

```bash
dotnet run
```

## Use

The API can be inspected using OpenAPI/Swagger on the `/swagger` endpoint.

The API has a single endpoint:

## `/predictions/load`

Return predicted load (simultaneous viewers) within a 24 hour window.

### Result

```typescript
{
  "from": string
  "to": string
  "movieIds": int[]
  "regions": string[]
  "viewers": int
}[]
```

### Parameters

* `?from={timestamp}`: Return only data within a time frame that begins after the given time.
* `?to={timestamp}`: Return only data within a time frame that begins before the given time.
* `?movieId={movieId}`: Return only data for the given movie
* `?region={region}`: Return only data for the given region

#### Formats

* `timestamp`: `string` of a date and time in the ISO 8601 format (e.g. `2020-01-01T12:00:00Z`)
* `movieId`: `int`, `1` through `10`
* `region`: `string`, one of `de`, `es`, `fr`, `gb`, `it`, `nl`

# INWT Challenge

## Data Engineering

### C4-Modell

#### C1

<image src="https://i.imgur.com/ncx22Wc.png" alt="C1" width="300" />

#### C2

<image src="https://i.imgur.com/fjL9iAE.png" alt="C2" width="300" />

#### C3

<image src="https://i.imgur.com/I39qGq8.png" alt="C3" width="300" />

### Datentransformation (statisch)

* Regelmäßige ETL-Prozesse fragen Daten aus den Datenbanken des Kunden ab, transformieren sie zu Feature-Daten und speichern sie in einer Feature-Datenbank
  * Entweder ein Prozess für Daten aus beiden Datenbanken (geringerer Maintenance-Overhead) oder zwei Prozesse (möglich, die vermutlich selten veränderten Metadaten seltener zu extrahieren) – vermutlich auch abhängig davon, welche Features extrahiert werden sollen
  * Trainingsdatenbank sollte für große, analytische Datenmengen geeignet sein, z.B. AWS Redshift
  * Täglicher Durchlauf sollte genügen, da die Daten vom Kunden täglich bereitgestellt werden

### Datentransformation (live)

* Ein langläufiger Prozess konsumiert den Live-Stream des Kunden (z.B. AWS Fargate via AWS MSK) und transformiert die Daten
  * Vermutlich Unterscheidung zwischen statischen Trainingsdaten und Parametern für die Prognoseerstellung; Daten können aber vermutlich in jedem Fall in der Trainingsdatenbank gespeichert werden, da sowohl Training, als auch Prognosegenerierung vermutlich nicht zeitkritisch sind

### Modellgenerierung

* Ein regelmäßig laufender Service generiert anhand der Trainingsdaten die neueste Version des Modells
  * Die Frequenzbestimmung liegt außerhalb des Scopes dieser Challenge
  * Ein möglicher Service wäre AWS SageMaker

### Prognoseerstellung

* Die Prognosedaten werden regelmäßig vom aktuellsten Modell generiert und in einer In-Memory-Datenbank für schnellen Zugriff durch den API-Server bereitgestellt
  * 15 Minuten wäre ein sinnvoller Default, da das auch die Auflösung der Prognosedaten ist
  * Die aus den Live-Daten transformierten Features dienen als Eingabeparameter für das Modell

## API

Aufgrund der geringen Anzahl von Anwendungsfällen genügt eine REST-like API, z.B. ASP.NET MVC in AWS AppRunner.

### Endpunkte & Aggregation

* Für den genannten Hauptanwendungsfall genügt ein Endpunkt, z.B. `/predictions/load`, da die Datenstruktur der Ausgabe immer gleich ist
  * Ohne weitere Parameter wäre die Ausgabe die erwartete Gesamtlast je 15-Minuten-Abschnitt für die kommenden 24 Stunden
  * Aggregationen, Filterung, etc. kann über Query-Parameter abgebildet werden:
    * `?group_by={movie|region}` erlaubt die Gruppierung nach Filmen und/oder Region der Nutzer
    * `?from={timestamp}&to={timestamp}` erlaubt die Einschränkung auf einen Zeitrahmen
    * `?movieId={movieId}` und `?region={regionId}` erlauben die Filterung nach bestimmten Filmen und/oder Regionen
  * Zusätzliche Endpunkte könnten je nach weiteren Anwendungsfällen hinzugefügt werden
  * Um die API und ihre Nutzung resistent gegenüber Änderungen zu machen, wäre die Implementierung von Versionierung wichtig

### Externe Ressourcen

* Die einzige feature-bedingte Ressource der API ist die Prognosedatenbank
* Technisch bedingt können weitere Abhängigkeiten dazukommen, z.B.:
  * Ein Request-Cache, um die Last durch häufig vorkommende Aggregationen zu reduzieren
    * Eine sinnvolle Lebenszeit des Caches wäre die Hälfte der Frequenz der Prognosegenerieung, um die bereitgestellten Prognosen möglichst aktuell zu halten (z.B. 7,5 Minuten)
    * Alternativ könnte der Cache bei Neuerstellung der Prognosedaten invalidiert werden
  * Ein Auth-Service, um nur berechtigen Diensten/Nutzern Zugriff zu gewähren
 
## Deployment

* Code sollte in einem Code-Repository (z.B. GitHub, GitLab, Azure DevOps) versionskontrolliert gesichert werden
  * Generell sollte versucht werden, für Deployment-Umgebungen Infrastructure-as-Code zu verwenden (z.B. mit GitOps, Terraform) um auch diese versionskontrolliert und reproduzierbar zu halten
* Diese Dienste ermöglichen auch CI um zu gewährleisten, dass kooperative Entwicklung möglichst reibungslos abläuft, z.B. durch Branch-Regeln, Code Reviews und flexibler Gestaltung von Pipelines um z.B. automatisierte Tests, Linting, Static Code Analysis, Test-Builds, etc. auszuführen
* Tooling für CD ist ebenso wichtig, z.B. Registries für Pakete oder Images, automatisiertes Deployment von Release-Branches, automatisiertes Rollback bei Problemen
  * Dank Infrastructure-as-Code können neben Software auch Umgebungen von CD profitieren
* Eine geeignete Branching-Strategie wäre für sowohl CI, als auch CD wichtig

### Beispiel-Pipeline

![Abbildung einer Beispiel-Pipeline](https://i.imgur.com/Vh29HEU.png)
<sup><https://mermaid.ai/d/da709528-00b3-4898-9a58-0f044a4f49ba></sup>

* In diesem Fall würden Deployment nach Staging oder Production z.B. nur bei Merge in den jeweiligen Branch ausgeführt, E2E-Tests nur bei Deployment oder manuell, etc.

### Änderungsresistenz

* Um die diversen Bestandteile der Softwarelandschaft änderungsresistent zu machen, sind folgende Maßnahmen hilfreich:
  * Rolling deployments (sollten die genannten AWS-Dienste unterstützen)
  * API-Versionierung der Prognose-API
  * Datenmigration für die Trainings- und Prognosedaten

### Alternative Deployment-Umgebungen

* Statt der Nutzung diverser AWS-Compute-Dienste (oder vergleichbaren Angeboten von Azure oder GCP) wäre auch die Verwendung eines Managed Kubernetes-Dienstes möglich
  * Vorteil: Wenig Anpassung nötig für den Wechsel zu einem anderen Cloud-Provider oder einem anderen Kubernetes-Cluster
  * Nachteile: Für ein Produkt dieser Größe eventuell nicht effizient in sowohl Entwicklungsaufwand, als auch Kosten; für Datenbanken und anderen Storage wären Cloud-Services bisher besser und günstiger
