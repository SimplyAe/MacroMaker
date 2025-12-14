using System;

namespace MacroEngine.Models
{
    /// <summary>
    /// Types of macro events that can be recorded
    /// </summary>
    public enum EventType
    {
        MouseMove,
        MouseLeftDown,
        MouseLeftUp,
        MouseRightDown,
        MouseRightUp,
        MouseMiddleDown,
        MouseMiddleUp,
        MouseWheel,
        KeyDown,
        KeyUp,
        Delay
    }

    /// <summary>
    /// Represents a single recorded event in a macro
    /// </summary>
    public class MacroEvent
    {
        /// <summary>
        /// Type of event
        /// </summary>
        public EventType Type { get; set; }

        /// <summary>
        /// X coordinate (for mouse events)
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y coordinate (for mouse events)
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Virtual key code (for keyboard events)
        /// </summary>
        public int KeyCode { get; set; }

        /// <summary>
        /// Mouse wheel delta (for wheel events)
        /// </summary>
        public int WheelDelta { get; set; }

        /// <summary>
        /// Timestamp in milliseconds from start of recording
        /// </summary>
        public double Timestamp { get; set; }

        /// <summary>
        /// Duration of the event in milliseconds (for delays)
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// Whether this event has been humanized
        /// </summary>
        public bool IsHumanized { get; set; }

        public MacroEvent()
        {
        }

        public MacroEvent(EventType type, double timestamp)
        {
            Type = type;
            Timestamp = timestamp;
        }

        public override string ToString()
        {
            return Type switch
            {
                EventType.MouseMove => $"MouseMove ({X}, {Y}) @ {Timestamp:F2}ms",
                EventType.KeyDown => $"KeyDown ({KeyCode}) @ {Timestamp:F2}ms",
                EventType.KeyUp => $"KeyUp ({KeyCode}) @ {Timestamp:F2}ms",
                EventType.Delay => $"Delay ({Duration:F2}ms) @ {Timestamp:F2}ms",
                _ => $"{Type} @ {Timestamp:F2}ms"
            };
        }
    }
}
