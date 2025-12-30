# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Race Horology is a timing system application for alpine ski races (and other racing sports), primarily used in German-speaking regions. It supports multiple timing devices: ALGE TdC8000/8001, ALGE Timy, Alpenhunde, Microgate Racetime 2, Rei 2, Rei Pro, and RT Pro.

## Build Commands

```bash
# Restore NuGet packages
nuget restore

# Debug build (x86)
msbuild /p:Configuration=Debug /p:Platform=x86 RaceHorology.sln

# Release build (x86)
msbuild /p:Configuration=Release /p:Platform=x86 RaceHorology.sln

# Build installer (WiX)
msbuild /p:Configuration=Release /p:Platform=x86 RaceHorologySetup
```

## Running Tests

```bash
# Run all unit tests (excluding hardware-dependent tests)
vstest.console.exe /Platform:x86 RaceHorologyLibTest\bin\x86\Debug\RaceHorologyLibTest.dll /TestCaseFilter:"TestCategory!=HardwareDependent&TestCategory!=IntegrationDsvOnline"

# Run specific test class
vstest.console.exe /Platform:x86 RaceHorologyLibTest\bin\x86\Debug\RaceHorologyLibTest.dll /Tests:TestClassName

# Run DSV online integration tests (requires internet)
vstest.console.exe /Platform:x86 RaceHorologyLibTest\bin\x86\Debug\RaceHorologyLibTest.dll /TestCaseFilter:"TestCategory=IntegrationDsvOnline"
```

Test categories to exclude in local development:
- `HardwareDependent` - Requires physical timing devices
- `IntegrationDsvOnline` - Requires DSV online connectivity

## Architecture

### Project Structure

- **RaceHorology/** - WPF desktop application (XAML UI with code-behind)
- **RaceHorologyLib/** - Core business logic library
- **RaceHorologyLibTest/** - MSTest unit tests
- **LiveTimingRM/, LiveTimingFIS/** - Live timing integrations (.NET Core)
- **RHAlgeTimyUSB/** - ALGE Timy USB driver
- **RaceHorologySetup/** - WiX installer project

### Core Library (RaceHorologyLib)

The library is organized around a central `AppDataModel` class split across multiple partial class files:
- `AppDataModel.cs` - Main data model orchestration
- `AppDataModelCalculations.cs` - Race timing calculations
- `AppDataModelDB.cs` - Database persistence layer
- `AppDataModelViews.cs` - Data views and transformations
- `AppDataModelDataTypes.cs` - Data structures

Key subsystems:
- **Database**: `Database.cs` implements `IAppDataModelDataBase` using OleDb for Microsoft Access (.mdb) files
- **Timing Devices**: Handlers in `ALGE*.cs`, `Microgate*.cs`, `TimingDeviceAlpenhunde.cs`
- **Import/Export**: `DSVImport.cs`, `DSVExport.cs`, `FISImport.cs` for federation data exchange
- **Calculations**: `DSVCalculations.cs`, `FISCalculations.cs` for scoring systems
- **Reports**: `PDFReports.cs` (iText7), `PrintCertificate.cs` for PDF generation

### Data Storage

Uses Microsoft Access (.mdb) databases. Template database is embedded in the assembly (`TemplateDB_Standard.mdb`).

## Technology Stack

- .NET Framework 4.8
- WPF (Windows Presentation Foundation)
- MSTest for unit testing
- iText7 for PDF generation
- NLog for logging
- ClosedXML for Excel handling

## Language Notes

- UI labels and many code comments are in German
- Database field names use German (e.g., "Vorname" = first name, "Nachname" = last name)
- DSV = Deutscher Skiverband (German Ski Association)
- FIS = International Ski Federation

## Branch Conventions

- `main` - Stable development, triggers pre-releases
- `release/*` - Release branches, trigger official releases
- `feature/*` - Feature development
- `bugfix/*` - Bug fixes
