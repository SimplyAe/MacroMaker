using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MacroEngine.Models
{
    /// <summary>
    /// Represents a complete macro with metadata and events
    /// </summary>
    public class Macro
    {
        /// <summary>
        /// Unique identifier for the macro
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Display name of the macro
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of what the macro does
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Tags for categorization and search
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// When the macro was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// When the macro was last modified
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// List of all events in the macro
        /// </summary>
        public List<MacroEvent> Events { get; set; }

        /// <summary>
        /// Playback speed multiplier (1.0 = normal speed)
        /// </summary>
        public double PlaybackSpeed { get; set; }

        /// <summary>
        /// Number of times to loop (0 = infinite)
        /// </summary>
        public int LoopCount { get; set; }

        /// <summary>
        /// Humanization level (0.0 = none, 1.0 = maximum)
        /// </summary>
        public double HumanizationLevel { get; set; }

        /// <summary>
        /// Hotkey assigned to this macro (virtual key code)
        /// </summary>
        public int? HotkeyCode { get; set; }

        /// <summary>
        /// Hotkey modifiers (Ctrl, Alt, Shift)
        /// </summary>
        public int? HotkeyModifiers { get; set; }

        /// <summary>
        /// Total duration of the macro in milliseconds
        /// </summary>
        [JsonIgnore]
        public double TotalDuration
        {
            get
            {
                if (Events == null || Events.Count == 0)
                    return 0;
                
                double maxTimestamp = 0;
                foreach (var evt in Events)
                {
                    if (evt.Timestamp > maxTimestamp)
                        maxTimestamp = evt.Timestamp;
                }
                return maxTimestamp;
            }
        }

        /// <summary>
        /// Number of events in the macro
        /// </summary>
        [JsonIgnore]
        public int EventCount => Events?.Count ?? 0;

        public Macro()
        {
            Id = Guid.NewGuid();
            Name = "New Macro";
            Description = "";
            Tags = new List<string>();
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
            Events = new List<MacroEvent>();
            PlaybackSpeed = 1.0;
            LoopCount = 1;
            HumanizationLevel = 0.0;
        }

        public Macro(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Creates a deep copy of this macro
        /// </summary>
        public Macro Clone()
        {
            var json = JsonConvert.SerializeObject(this);
            var clone = JsonConvert.DeserializeObject<Macro>(json);
            clone!.Id = Guid.NewGuid();
            clone.Name = Name + " (Copy)";
            clone.CreatedDate = DateTime.Now;
            clone.ModifiedDate = DateTime.Now;
            return clone;
        }
    }
}
