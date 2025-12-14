using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HotkeyManager
{
    /// <summary>
    /// Manages global hotkeys using Win32 RegisterHotKey API
    /// </summary>
    public class GlobalHotkey : IDisposable
    {
        // Win32 API imports
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Modifier keys
        [Flags]
        public enum Modifiers : uint
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Win = 8
        }

        private readonly IntPtr _windowHandle;
        private readonly int _id;
        private bool _isRegistered;

        /// <summary>
        /// Event fired when the hotkey is pressed
        /// </summary>
        public event EventHandler? HotkeyPressed;

        /// <summary>
        /// The key code for this hotkey
        /// </summary>
        public Keys Key { get; private set; }

        /// <summary>
        /// The modifier keys for this hotkey
        /// </summary>
        public Modifiers Modifier { get; private set; }

        public GlobalHotkey(IntPtr windowHandle, int id, Modifiers modifiers, Keys key)
        {
            _windowHandle = windowHandle;
            _id = id;
            Modifier = modifiers;
            Key = key;
        }

        /// <summary>
        /// Registers the hotkey with the system
        /// </summary>
        public bool Register()
        {
            if (_isRegistered)
                return true;

            _isRegistered = RegisterHotKey(_windowHandle, _id, (uint)Modifier, (uint)Key);
            return _isRegistered;
        }

        /// <summary>
        /// Unregisters the hotkey
        /// </summary>
        public void Unregister()
        {
            if (!_isRegistered)
                return;

            UnregisterHotKey(_windowHandle, _id);
            _isRegistered = false;
        }

        /// <summary>
        /// Called when WM_HOTKEY message is received
        /// </summary>
        public void OnHotkeyPressed()
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            Unregister();
        }

        public override string ToString()
        {
            string modStr = "";
            if (Modifier.HasFlag(Modifiers.Control)) modStr += "Ctrl+";
            if (Modifier.HasFlag(Modifiers.Alt)) modStr += "Alt+";
            if (Modifier.HasFlag(Modifiers.Shift)) modStr += "Shift+";
            if (Modifier.HasFlag(Modifiers.Win)) modStr += "Win+";
            
            return modStr + Key.ToString();
        }
    }
}
