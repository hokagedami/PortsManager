# PortsManager

A cross-platform desktop application to identify which local processes are bound to network ports and terminate them when needed.

## Features

- List all listening TCP/UDP ports on the local machine
- View process details: port, protocol, process name, PID, and status
- Search/filter by port number, process name, or PID
- Graceful process termination with optional forceful termination
- Auto-refresh with configurable interval
- Dark/Light/System theme support
- Cross-platform support: Windows, macOS, and Linux

## Requirements

- .NET 8.0 SDK

## Building

```bash
dotnet restore
dotnet build
```

## Running

```bash
dotnet run --project src/PortsManager.Desktop
```

## Desktop UI Features

- **Toolbar** with icon buttons for Refresh, Terminate, Copy, and Settings
- **Search bar** to filter ports by port number, process name, or PID
- **Port list** showing Port, Protocol, Process, PID, and Status columns
- **Context menu** with View Details, Terminate, and Copy options
- **Double-click** a row to view detailed port information
- **Settings page** with:
  - Auto-refresh toggle and interval configuration
  - Behavior options (confirm before terminate, show system processes)
  - Theme selection (System, Light, Dark)

## Project Structure

```
PortsManager/
├── src/
│   ├── PortsManager.Core/      # Shared business logic
│   │   ├── Models/             # Data models
│   │   └── Services/           # Port scanning and process termination
│   └── PortsManager.Desktop/   # Avalonia desktop UI
│       ├── Assets/             # Application icons
│       ├── Models/             # App settings
│       ├── ViewModels/         # MVVM view models
│       └── Views/              # Dialog windows
└── README.md
```

## Platform Implementation

- **Windows**: Uses `netstat` command
- **macOS**: Uses `lsof` command
- **Linux**: Uses `ss` command

## License

MIT License - see [LICENSE.txt](LICENSE.txt)
