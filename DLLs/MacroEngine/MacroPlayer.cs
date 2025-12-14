using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MacroEngine.Models;

namespace MacroEngine
{
    /// <summary>
    /// Handles playback of recorded macros with precise timing
    /// </summary>
    public class MacroPlayer
    {
        private bool _isPlaying;
        private CancellationTokenSource? _cancellationTokenSource;
        private readonly object _lock = new object();

        /// <summary>
        /// Event fired when playback starts
        /// </summary>
        public event EventHandler? PlaybackStarted;

        /// <summary>
        /// Event fired when playback stops
        /// </summary>
        public event EventHandler? PlaybackStopped;

        /// <summary>
        /// Event fired when an event is about to be played
        /// </summary>
        public event EventHandler<MacroEvent>? EventPlaying;

        /// <summary>
        /// Event fired to request input simulation (handled by main app)
        /// </summary>
        public event EventHandler<MacroEvent>? SimulateInput;

        /// <summary>
        /// Event fired with playback progress (0.0 to 1.0)
        /// </summary>
        public event EventHandler<double>? ProgressChanged;

        /// <summary>
        /// Whether playback is currently active
        /// </summary>
        public bool IsPlaying
        {
            get { lock (_lock) return _isPlaying; }
        }

        /// <summary>
        /// Starts playing a macro
        /// </summary>
        public async Task PlayAsync(Macro macro)
        {
            if (macro == null || macro.Events.Count == 0)
                throw new ArgumentException("Macro is null or has no events");

            lock (_lock)
            {
                if (_isPlaying)
                    throw new InvalidOperationException("Already playing a macro");

                _isPlaying = true;
                _cancellationTokenSource = new CancellationTokenSource();
            }

            PlaybackStarted?.Invoke(this, EventArgs.Empty);

            try
            {
                int loopCount = macro.LoopCount == 0 ? int.MaxValue : macro.LoopCount;
                
                for (int loop = 0; loop < loopCount; loop++)
                {
                    if (_cancellationTokenSource?.Token.IsCancellationRequested == true)
                        break;

                    await PlayMacroOnceAsync(macro, _cancellationTokenSource.Token);
                }
            }
            finally
            {
                lock (_lock)
                {
                    _isPlaying = false;
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                }

                PlaybackStopped?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Stops playback
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                _cancellationTokenSource?.Cancel();
            }
        }

        private async Task PlayMacroOnceAsync(Macro macro, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            double lastTimestamp = 0;

            for (int i = 0; i < macro.Events.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var evt = macro.Events[i];
                
                // Calculate delay needed to maintain timing
                double targetTime = evt.Timestamp / macro.PlaybackSpeed;
                double currentTime = stopwatch.Elapsed.TotalMilliseconds;
                double delay = targetTime - currentTime;

                if (delay > 0)
                {
                    await Task.Delay((int)delay, cancellationToken);
                }

                // Fire event and simulate input
                EventPlaying?.Invoke(this, evt);
                SimulateInput?.Invoke(this, evt);

                // Report progress
                double progress = (double)(i + 1) / macro.Events.Count;
                ProgressChanged?.Invoke(this, progress);

                lastTimestamp = evt.Timestamp;
            }
        }
    }
}
