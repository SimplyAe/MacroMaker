# Macro Maker

A powerful, smooth, and realistic macro recorder and player with AI-powered humanization.

## Features

- **Sub-millisecond Recording Precision**: High-resolution timestamps for accurate playback
- **AI-Powered Humanization**: Python-based algorithms add realistic variance to timing and positions
- **Global Hotkeys**: System-wide hotkeys (F6/F7/F8) for recording and playback
- **Modern UI**: Glassmorphism design with smooth animations
- **Import/Export**: Save and share macros easily
- **Pattern Detection**: AI analyzes macros for optimization opportunities
- **Speed Control**: Adjust playback speed from 0.1x to 10x
- **Loop Support**: Repeat macros a specific number of times or infinitely

## Architecture

### C++ DLLs (Native Performance)
- **InputHook.dll**: Low-level mouse and keyboard hooks (WH_MOUSE_LL, WH_KEYBOARD_LL)
- **InputSimulator.dll**: SendInput wrapper with smooth interpolation

### C# DLLs (Business Logic)
- **MacroEngine.dll**: Recording and playback engine
- **HotkeyManager.dll**: Global hotkey registration

### Python AI Engine
- **humanizer.py**: Bézier curves and Gaussian noise for natural playback
- **pattern_analyzer.py**: Pattern detection and optimization suggestions

## Installation

### Prerequisites
- Windows 10/11
- .NET 8.0 SDK
- Visual Studio 2022 (with C++ and C# workloads)
- Python 3.8 or higher

### Building from Source

1. **Clone the repository**
   ```bash
   cd C:\Users\DES\Desktop\Maker
   ```

2. **Install Python dependencies**
   ```bash
   cd python
   pip install -r requirements.txt
   cd ..
   ```

3. **Build the solution**
   ```bash
   dotnet build MacroMaker.sln --configuration Release
   ```

   Or open `MacroMaker.sln` in Visual Studio and build (Ctrl+Shift+B).

4. **Run the application**
   ```bash
   dotnet run --project MacroMaker/MacroMaker.csproj
   ```

## Usage

### Recording a Macro
1. Click the **Record** button or press **F6**
2. Perform your mouse and keyboard actions
3. Click **Stop** or press **F7** when done
4. The macro will be saved automatically

### Playing a Macro
1. Select a macro from the list
2. Adjust playback settings (speed, loops, humanization)
3. Click **Play** or press **F8**

### Editing a Macro
1. Select a macro from the list
2. Edit the name and description
3. View the event timeline
4. Click **Save** to persist changes

### Importing/Exporting
- **Import**: Click "Import" and select a `.macro` file
- **Export**: Select a macro and click "Export"

## Hotkeys

- **F6**: Start recording
- **F7**: Stop recording
- **F8**: Play selected macro

## Settings

- **Playback Speed**: 0.1x to 10x multiplier
- **Loop Count**: Number of repetitions (0 = infinite)
- **Humanization Level**: 0% to 100% variance

## File Structure

```
Maker/
├── DLLs/
│   ├── InputHook/          # C++ input hook DLL
│   ├── InputSimulator/     # C++ input simulator DLL
│   ├── MacroEngine/        # C# macro engine DLL
│   └── HotkeyManager/      # C# hotkey manager DLL
├── MacroMaker/             # Main WPF application
│   ├── Services/           # Application services
│   ├── Resources/          # Styles and resources
│   └── MainWindow.xaml     # Main UI
├── python/                 # Python AI engine
│   ├── humanizer.py
│   ├── pattern_analyzer.py
│   └── requirements.txt
└── SavedMacros/            # Saved macro files
```

## Technical Details

### Recording
- Uses Win32 `SetWindowsHookEx` with `WH_MOUSE_LL` and `WH_KEYBOARD_LL`
- High-resolution timestamps via `std::chrono::high_resolution_clock`
- Thread-safe event queue

### Playback
- Precise timing control with async/await
- SendInput API for reliable input simulation
- Smooth mouse interpolation with cubic splines

### Humanization
- Gaussian noise for timing variance (±5ms)
- Position jitter for mouse events (±2px)
- Bézier curve generation for natural mouse paths
- Micro-delays between events

## Troubleshooting

### DLL Not Found
Ensure all DLLs are in the same directory as the executable:
- `InputHook.dll`
- `InputSimulator.dll`
- `MacroEngine.dll`
- `HotkeyManager.dll`

### Hotkeys Not Working
- Make sure no other application is using F6/F7/F8
- Run the application as Administrator if needed

### Python Errors
Install Python dependencies:
```bash
cd python
pip install -r requirements.txt
```

## License

This project is for educational purposes.

## Contributing

Contributions are welcome! Please feel free to submit pull requests.
