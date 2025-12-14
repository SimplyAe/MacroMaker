using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using MacroEngine;
using MacroEngine.Models;
using MacroMaker.Services;
using HotkeyManager;
using Microsoft.Win32;

namespace MacroMaker
{
    public partial class MainWindow : Window
    {
        private DllManager _dllManager;
        private MacroRecorder _recorder;
        private MacroPlayer _player;
        private StorageService _storage;
        private List<Macro> _macros;
        private Macro? _selectedMacro;
        private GlobalHotkey? _recordHotkey;
        private GlobalHotkey? _stopHotkey;
        private GlobalHotkey? _playHotkey;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize services
            _dllManager = new DllManager();
            _recorder = new MacroRecorder();
            _player = new MacroPlayer();
            _storage = new StorageService();
            _macros = new List<Macro>();

            // Wire up events
            _dllManager.MouseMoved += OnMouseMoved;
            _dllManager.MouseButton += OnMouseButton;
            _dllManager.Keyboard += OnKeyboard;

            _recorder.RecordingStarted += OnRecordingStarted;
            _recorder.RecordingStopped += OnRecordingStopped;
            _recorder.EventRecorded += OnEventRecorded;

            _player.PlaybackStarted += OnPlaybackStarted;
            _player.PlaybackStopped += OnPlaybackStopped;
            _player.SimulateInput += OnSimulateInput;
            _player.ProgressChanged += OnProgressChanged;

            // Load macros
            LoadMacros();

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Register global hotkeys
            var windowHandle = new WindowInteropHelper(this).Handle;
            _recordHotkey = new GlobalHotkey(windowHandle, 1, GlobalHotkey.Modifiers.None, System.Windows.Forms.Keys.F6);
            _stopHotkey = new GlobalHotkey(windowHandle, 2, GlobalHotkey.Modifiers.None, System.Windows.Forms.Keys.F7);
            _playHotkey = new GlobalHotkey(windowHandle, 3, GlobalHotkey.Modifiers.None, System.Windows.Forms.Keys.F8);

            _recordHotkey.HotkeyPressed += (s, args) => RecordButton_Click(s, new RoutedEventArgs());
            _stopHotkey.HotkeyPressed += (s, args) => StopButton_Click(s, new RoutedEventArgs());
            _playHotkey.HotkeyPressed += (s, args) => PlayButton_Click(s, new RoutedEventArgs());

            _recordHotkey.Register();
            _stopHotkey.Register();
            _playHotkey.Register();

            // Set up window message hook for hotkeys
            HwndSource source = HwndSource.FromHwnd(windowHandle);
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (id == 1) _recordHotkey?.OnHotkeyPressed();
                else if (id == 2) _stopHotkey?.OnHotkeyPressed();
                else if (id == 3) _playHotkey?.OnHotkeyPressed();
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _dllManager?.StopAllHooks();
            _recordHotkey?.Dispose();
            _stopHotkey?.Dispose();
            _playHotkey?.Dispose();
            _dllManager?.Dispose();
        }

        private void LoadMacros()
        {
            _macros = _storage.LoadAllMacros();
            MacroListBox.ItemsSource = _macros;
            UpdateStats();
        }

        private void UpdateStats()
        {
            StatsText.Text = $"{_macros.Count} macros loaded";
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (_recorder.IsRecording)
                return;

            _dllManager.StartHooks();
            _recorder.StartRecording("New Macro " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            
            RecordButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            PlayButton.IsEnabled = false;
            StatusText.Text = "Recording...";
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_recorder.IsRecording)
                return;

            var macro = _recorder.StopRecording();
            _dllManager.StopAllHooks();

            _macros.Insert(0, macro);
            MacroListBox.Items.Refresh();
            MacroListBox.SelectedItem = macro;

            _storage.SaveMacro(macro);

            RecordButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            PlayButton.IsEnabled = true;
            StatusText.Text = $"Recording stopped. {macro.EventCount} events captured.";
            UpdateStats();
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMacro == null || _player.IsPlaying)
                return;

            // Apply settings
            _selectedMacro.PlaybackSpeed = SpeedSlider.Value;
            if (int.TryParse(LoopCountBox.Text, out int loopCount))
            {
                _selectedMacro.LoopCount = loopCount;
            }

            RecordButton.IsEnabled = false;
            PlayButton.IsEnabled = false;
            StatusText.Text = "Playing macro...";

            await _player.PlayAsync(_selectedMacro);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMacro == null)
                return;

            _selectedMacro.Name = MacroNameBox.Text;
            _selectedMacro.Description = MacroDescriptionBox.Text;

            _storage.SaveMacro(_selectedMacro);
            MacroListBox.Items.Refresh();

            StatusText.Text = "Macro saved.";
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMacro == null)
                return;

            var result = MessageBox.Show($"Are you sure you want to delete '{_selectedMacro.Name}'?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _storage.DeleteMacro(_selectedMacro);
                _macros.Remove(_selectedMacro);
                MacroListBox.Items.Refresh();
                _selectedMacro = null;
                ClearMacroDetails();
                UpdateStats();
                StatusText.Text = "Macro deleted.";
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Macro Files (*.macro)|*.macro|All Files (*.*)|*.*",
                Title = "Import Macro"
            };

            if (dialog.ShowDialog() == true)
            {
                var macro = _storage.ImportMacro(dialog.FileName);
                if (macro != null)
                {
                    _macros.Insert(0, macro);
                    MacroListBox.Items.Refresh();
                    UpdateStats();
                    StatusText.Text = $"Imported '{macro.Name}'.";
                }
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedMacro == null)
                return;

            var dialog = new SaveFileDialog
            {
                Filter = "Macro Files (*.macro)|*.macro",
                FileName = _selectedMacro.Name + ".macro",
                Title = "Export Macro"
            };

            if (dialog.ShowDialog() == true)
            {
                _storage.ExportMacro(_selectedMacro, dialog.FileName);
                StatusText.Text = $"Exported '{_selectedMacro.Name}'.";
            }
        }

        private void MacroListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedMacro = MacroListBox.SelectedItem as Macro;
            if (_selectedMacro != null)
            {
                DisplayMacroDetails(_selectedMacro);
                PlayButton.IsEnabled = !_player.IsPlaying;
            }
            else
            {
                ClearMacroDetails();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                MacroListBox.ItemsSource = _macros;
            }
            else
            {
                MacroListBox.ItemsSource = _macros.Where(m =>
                    m.Name.ToLower().Contains(searchText) ||
                    m.Description.ToLower().Contains(searchText)).ToList();
            }
        }

        private void DisplayMacroDetails(Macro macro)
        {
            MacroNameBox.Text = macro.Name;
            MacroDescriptionBox.Text = macro.Description;
            EventListBox.ItemsSource = macro.Events.Select(e => e.ToString()).ToList();
            SpeedSlider.Value = macro.PlaybackSpeed;
            LoopCountBox.Text = macro.LoopCount.ToString();
            HumanizationSlider.Value = macro.HumanizationLevel;
        }

        private void ClearMacroDetails()
        {
            MacroNameBox.Text = "";
            MacroDescriptionBox.Text = "";
            EventListBox.ItemsSource = null;
        }

        // Input hook event handlers
        private void OnMouseMoved(object? sender, (int x, int y, double timestamp) e)
        {
            if (_recorder.IsRecording)
            {
                _recorder.RecordMouseMove(e.x, e.y);
            }
        }

        private void OnMouseButton(object? sender, (int button, bool isDown, int x, int y, double timestamp) e)
        {
            if (_recorder.IsRecording)
            {
                EventType eventType = e.button switch
                {
                    0 => e.isDown ? EventType.MouseLeftDown : EventType.MouseLeftUp,
                    1 => e.isDown ? EventType.MouseRightDown : EventType.MouseRightUp,
                    2 => e.isDown ? EventType.MouseMiddleDown : EventType.MouseMiddleUp,
                    _ => EventType.MouseLeftDown
                };
                _recorder.RecordMouseButton(eventType, e.x, e.y);
            }
        }

        private void OnKeyboard(object? sender, (int keyCode, bool isDown, double timestamp) e)
        {
            if (_recorder.IsRecording)
            {
                _recorder.RecordKeyboard(e.isDown ? EventType.KeyDown : EventType.KeyUp, e.keyCode);
            }
        }

        // Recorder event handlers
        private void OnRecordingStarted(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = "Recording started...";
            });
        }

        private void OnRecordingStopped(object? sender, Macro e)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = $"Recording stopped. {e.EventCount} events captured.";
            });
        }

        private void OnEventRecorded(object? sender, MacroEvent e)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = $"Recording... {_recorder.EventCount} events";
            });
        }

        // Player event handlers
        private void OnPlaybackStarted(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = "Playing macro...";
            });
        }

        private void OnPlaybackStopped(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                RecordButton.IsEnabled = true;
                PlayButton.IsEnabled = true;
                StatusText.Text = "Playback complete.";
            });
        }

        private void OnSimulateInput(object? sender, MacroEvent e)
        {
            _dllManager.SimulateEvent(e);
        }

        private void OnProgressChanged(object? sender, double progress)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = $"Playing... {progress:P0}";
            });
        }
    }
}
