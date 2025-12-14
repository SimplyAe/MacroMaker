using System;
using System.Collections.Generic;
using System.Diagnostics;
using MacroEngine.Models;

namespace MacroEngine
{
    /// <summary>
    /// Handles recording of macro events from input hooks
    /// </summary>
    public class MacroRecorder
    {
        private Macro? _currentMacro;
        private Stopwatch? _recordingTimer;
        private bool _isRecording;
        private readonly object _lock = new object();

        /// <summary>
        /// Event fired when recording starts
        /// </summary>
        public event EventHandler? RecordingStarted;

        /// <summary>
        /// Event fired when recording stops
        /// </summary>
        public event EventHandler<Macro>? RecordingStopped;

        /// <summary>
        /// Event fired when a new event is recorded
        /// </summary>
        public event EventHandler<MacroEvent>? EventRecorded;

        /// <summary>
        /// Whether recording is currently active
        /// </summary>
        public bool IsRecording
        {
            get { lock (_lock) return _isRecording; }
        }

        /// <summary>
        /// Number of events recorded in current session
        /// </summary>
        public int EventCount
        {
            get { lock (_lock) return _currentMacro?.Events.Count ?? 0; }
        }

        /// <summary>
        /// Current recording duration in milliseconds
        /// </summary>
        public double RecordingDuration
        {
            get { lock (_lock) return _recordingTimer?.Elapsed.TotalMilliseconds ?? 0; }
        }

        /// <summary>
        /// Starts recording a new macro
        /// </summary>
        public void StartRecording(string macroName = "New Macro")
        {
            lock (_lock)
            {
                if (_isRecording)
                    throw new InvalidOperationException("Already recording");

                _currentMacro = new Macro(macroName);
                _recordingTimer = Stopwatch.StartNew();
                _isRecording = true;
            }

            RecordingStarted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Stops recording and returns the completed macro
        /// </summary>
        public Macro StopRecording()
        {
            Macro? completedMacro;

            lock (_lock)
            {
                if (!_isRecording)
                    throw new InvalidOperationException("Not currently recording");

                _recordingTimer?.Stop();
                _isRecording = false;
                completedMacro = _currentMacro;
                _currentMacro = null;
                _recordingTimer = null;
            }

            if (completedMacro != null)
            {
                RecordingStopped?.Invoke(this, completedMacro);
            }

            return completedMacro!;
        }

        /// <summary>
        /// Records a mouse move event
        /// </summary>
        public void RecordMouseMove(int x, int y)
        {
            RecordEvent(new MacroEvent(EventType.MouseMove, GetCurrentTimestamp())
            {
                X = x,
                Y = y
            });
        }

        /// <summary>
        /// Records a mouse button event
        /// </summary>
        public void RecordMouseButton(EventType type, int x, int y)
        {
            RecordEvent(new MacroEvent(type, GetCurrentTimestamp())
            {
                X = x,
                Y = y
            });
        }

        /// <summary>
        /// Records a keyboard event
        /// </summary>
        public void RecordKeyboard(EventType type, int keyCode)
        {
            RecordEvent(new MacroEvent(type, GetCurrentTimestamp())
            {
                KeyCode = keyCode
            });
        }

        /// <summary>
        /// Records a mouse wheel event
        /// </summary>
        public void RecordMouseWheel(int delta, int x, int y)
        {
            RecordEvent(new MacroEvent(EventType.MouseWheel, GetCurrentTimestamp())
            {
                X = x,
                Y = y,
                WheelDelta = delta
            });
        }

        private void RecordEvent(MacroEvent evt)
        {
            lock (_lock)
            {
                if (!_isRecording || _currentMacro == null)
                    return;

                _currentMacro.Events.Add(evt);
            }

            EventRecorded?.Invoke(this, evt);
        }

        private double GetCurrentTimestamp()
        {
            lock (_lock)
            {
                return _recordingTimer?.Elapsed.TotalMilliseconds ?? 0;
            }
        }
    }
}
