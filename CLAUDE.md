# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Race Horology is a Windows timing application for alpine ski races (and other racing sports), used mainly in German-speaking regions. Its feature set is modelled after DSVAlpin. Supported timing devices: ALGE TdC8000/8001, ALGE Timy, Alpenhunde, Microgate Racetime 2 / Rei 2 / Rei Pro / RT Pro, TagHeuer.

License: GNU AGPL v3. Every source file starts with the bilingual (English/German) AGPL header `Copyright (C) 2019 - 2026 by Sven Flossmann & Co-Authors (CREDITS.TXT)` — copy it into new `.cs` files.

## Build

Windows only; requires Visual Studio / MSBuild, NuGet and WiX Toolset v3 (for the installer). Always build with `Platform=x86` — the database layer uses the 32-bit `Microsoft.Jet.OLEDB.4.0` provider.

```bash
nuget restore
msbuild /p:Configuration=Debug /p:Platform=x86 RaceHorology.sln
msbuild /p:Configuration=Release /p:Platform=x86 RaceHorology.sln
msbuild /p:Configuration=Release /p:Platform=x86 RaceHorologySetup   # MSI installer
```

## Tests

MSTest, run via `vstest.console.exe` against the built DLL (build first):

```bash
# All tests CI runs by default
vstest.console.exe /Platform:x86 RaceHorologyLibTest\bin\x86\Debug\RaceHorologyLibTest.dll /TestCaseFilter:"TestCategory!=HardwareDependent&TestCategory!=IntegrationDsvOnline"

# Single class or method
vstest.console.exe /Platform:x86 RaceHorologyLibTest\bin\x86\Debug\RaceHorologyLibTest.dll /Tests:DSVImportTest
```

- Test categories: `HardwareDependent` (needs physical devices, never run in CI) and `IntegrationDsvOnline` (needs internet; runs as a separate CI job).
- Test data (`.mdb` databases, race `.config` files, device logs, import files) lives in `RaceHorologyLibTest/TestDataBases/` and is pulled in per test via `[DeploymentItem(...)]`. Use `TestUtilities.CreateWorkingFileFrom(...)` to work on a copy rather than the original file.
- PDF report tests generate a PDF and compare it with a checked-in reference ("shall") PDF using iText's `CompareTool` (`TestUtilities.GenerateAndCompareAgainstPdf`). When report layout changes intentionally, the reference PDFs must be regenerated and committed.
- `TestDataGenerator` and `DummyDataBase` in `TestUtilities.cs` build in-memory models without a database.

## Architecture

### Solution projects

| Project | Purpose |
|---|---|
| `RaceHorology/` | WPF desktop app (.NET Framework 4.8), XAML user controls (`*UC.xaml`) and dialogs (`*Dlg.xaml`) with code-behind |
| `RaceHorologyLib/` | All business logic: data model, DB access, timing devices, import/export, calculations, PDF reports |
| `RaceHorologyLibTest/` | MSTest tests for the library |
| `LiveTimingRM/`, `LiveTimingFIS/` | Live-timing uploads (Rennmeldung / FIS live timing); `LiveTimingRM` is SDK-style targeting `net48` |
| `RHAlgeTimyUSB/` | Wrapper around the native ALGE Timy USB DLLs |
| `RHDevTools/` | Console tool with DSVAlpin database helpers for development |
| `RaceHorologySetup/` | WiX installer (`Product.wxs`) |

### Core library (`RaceHorologyLib`)

- **Data model**: `AppDataModel` (in `AppDataModel.cs`) is the root object; it owns `Race` objects, which own `RaceRun`s. Participants, classes, groups and categories are in `AppDataModelDataTypes.cs` / `ClassesGroupsCategories.cs`. The model uses `INotifyPropertyChanged` and observable collections that the WPF UI binds to directly.
- **View providers** (`AppDataModelViews.cs`): start lists and results are computed by provider classes (`*StartListViewProvider`, `RaceRunResultViewProvider`, `RaceResultViewProvider`, plus DSV/FIS/points/team variants). The race configuration selects which provider is used.
- **Race configuration** (`RaceConfiguration.cs`): per-race settings (runs, sorting, result mode, points) stored as `.config` next to the database; presets ship in `raceconfigpresets/`.
- **Persistence**: `Database.cs` implements `IAppDataModelDataBase` (`DatabaseInterface.cs`) against Microsoft Access `.mdb` files (DSVAlpin-compatible schema). New files are created from the embedded `dbtemplates/TemplateDB_Standard.mdb`.
- **Timing devices**: `ALGETdC8001TimeMeasurement.cs`, `ALGETimy.cs`, `MicrogateV1/V2TimeMeasurement.cs`, `TimingDeviceAlpenhunde.cs`, `TagHeuer.cs`; common abstractions in `LiveTimingMeasurement.cs`. Hand timing (`HandTiming.cs`) and time import (`ImportTime.cs`) fill gaps.
- **Import/export**: `Import.cs`, `DSVImport.cs`, `FISImport.cs`, `Export.cs`, `DSVExport.cs`.
- **Calculations**: `DSVCalculations.cs`, `FISCalculations.cs` (points/penalties).
- **Reports**: `PDFReports.cs` (iText7, fonts in `resources/`), `PrintCertificate.cs`, referee reports (`RefereeReport*.cs`, `RefereeProtocol.cs`).
- **Live data server**: `DSVAlpin2HTTPServer.cs` serves start lists/results over HTTP and pushes updates via WebSockets (`/api/LiveData`, websocket-sharp).

## Conventions

- Indentation: 2 spaces (`.editorconfig`), braces on their own line.
- UI text, many comments and DB field names are German (e.g. `Vorname`, `Nachname`, `Durchgang` = run). DSV = Deutscher Skiverband, FIS = International Ski Federation.
- `Documents/` contains specs (DSV points calculation, DSV results XML, JSON API, user manual).

## CI and branches

GitHub Actions (`.github/workflows/main.yml`) builds Debug + Release, builds and test-installs the MSI, and runs tests on pushes to `main`, `release/**`, `feature/**`, `bugfix/**` and on PRs to `main`.

- Version is computed by semantic-version from `v*` tags, bumped on every commit; put `(release-MINOR)` or `(release-MAJOR)` in a commit message to bump those parts.
- `main` publishes a pre-release; `release/*` publishes an official GitHub release.
- Work on `feature/*` or `bugfix/*` branches and open PRs against `main`.
