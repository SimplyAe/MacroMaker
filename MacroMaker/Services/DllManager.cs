using System;
using System.Runtime.InteropServices;
using MacroEngine.Models;

namespace MacroMaker.Services
{
    /// <summary>
    /// Manages loading and interaction with native C++ DLLs
    /// </summary>
    public class DllManager : IDisposable
    {
        // InputHook.dll P/Invoke declarations
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void MouseMoveCallback(int x, int y, double timestamp);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void MouseButtonCallback(int button, bool isDown, int x, int y, double timestamp);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void KeyboardCallback(int keyCode, bool isDown, double timestamp);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void MouseWheelCallback(int delta, int x, int y, double timestamp);

        [DllImport("InputHook.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool StartMouseHook(MouseMoveCallback moveCallback, MouseButtonCallback buttonCallback, MouseWheelCallback wheelCallback);

        [DllImport("InputHook.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool StartKeyboardHook(KeyboardCallback keyCallback);

        [DllImport("InputHook.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void StopHooks();

        // InputSimulator.dll P/Invoke declarations
        [DllImport("InputSimulator.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void MoveMouse(int x, int y);

        [DllImport("InputSimulator.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void MoveMouseSmooth(int targetX, int targetY, int steps, int delayMs);

        [DllImport("InputSimulator.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void MouseButtonDown(int button);

        [DllImport("InputSimulator.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void MouseButtonUp(int button);

        [DllImport("InputSimulator.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void MouseClick(int button);

        [DllImport("InputSimulator.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void MouseWheel(int delta);

        [DllImport("InputSimulator.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void KeyDown(int keyCode);

        [DllImport("InputSimulator.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void KeyUp(int keyCode);

        [DllImport("InputSimulator.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void PreciseSleep(double milliseconds);

        // Events
        public event EventHandler<(int x, int y, double timestamp)>? MouseMoved;
        public event EventHandler<(int button, bool isDown, int x, int y, double timestamp)>? MouseButton;
        public event EventHandler<(int keyCode, bool isDown, double timestamp)>? Keyboard;
        public event EventHandler<(int delta, int x, int y, double timestamp)>? MouseWheelEvent;

        private MouseMoveCallback? _mouseMoveCallback;
        private MouseButtonCallback? _mouseButtonCallback;
        private KeyboardCallback? _keyboardCallback;
        private MouseWheelCallback? _mouseWheelCallback;

        public DllManager()
        {
            // Keep references to prevent garbage collection
            _mouseMoveCallback = OnMouseMove;
            _mouseButtonCallback = OnMouseButton;
            _keyboardCallback = OnKeyboard;
            _mouseWheelCallback = OnMouseWheel;
        }

        public bool StartHooks()
        {
            bool mouseSuccess = StartMouseHook(_mouseMoveCallback!, _mouseButtonCallback!, _mouseWheelCallback!);
            bool keyboardSuccess = StartKeyboardHook(_keyboardCallback!);
            return mouseSuccess && keyboardSuccess;
        }

        public void StopAllHooks()
        {
            StopHooks();
        }

        public void SimulateEvent(MacroEvent evt)
        {
            switch (evt.Type)
            {
                case EventType.MouseMove:
                    MoveMouse(evt.X, evt.Y);
                    break;

                case EventType.MouseLeftDown:
                    MouseButtonDown(0);
                    break;

                case EventType.MouseLeftUp:
                    MouseButtonUp(0);
                    break;

                case EventType.MouseRightDown:
                    MouseButtonDown(1);
                    break;

                case EventType.MouseRightUp:
                    MouseButtonUp(1);
                    break;

                case EventType.MouseMiddleDown:
                    MouseButtonDown(2);
                    break;

                case EventType.MouseMiddleUp:
                    MouseButtonUp(2);
                    break;

                case EventType.MouseWheel:
                    MouseWheel(evt.WheelDelta);
                    break;

                case EventType.KeyDown:
                    KeyDown(evt.KeyCode);
                    break;

                case EventType.KeyUp:
                    KeyUp(evt.KeyCode);
                    break;

                case EventType.Delay:
                    PreciseSleep(evt.Duration);
                    break;
            }
        }

        private void OnMouseMove(int x, int y, double timestamp)
        {
            MouseMoved?.Invoke(this, (x, y, timestamp));
        }

        private void OnMouseButton(int button, bool isDown, int x, int y, double timestamp)
        {
            MouseButton?.Invoke(this, (button, isDown, x, y, timestamp));
        }

        private void OnKeyboard(int keyCode, bool isDown, double timestamp)
        {
            Keyboard?.Invoke(this, (keyCode, isDown, timestamp));
        }

        private void OnMouseWheel(int delta, int x, int y, double timestamp)
        {
            MouseWheelEvent?.Invoke(this, (delta, x, y, timestamp));
        }

        public void Dispose()
        {
            StopAllHooks();
        }
    }
}
